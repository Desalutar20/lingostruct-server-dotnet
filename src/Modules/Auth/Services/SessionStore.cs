using System.Text.Json;
using src.Config;
using src.Modules.Auth.Services;
using src.Modules.Shared.Constants;
using src.Modules.Users.Domain;
using StackExchange.Redis;

internal sealed class SessionStore(IConnectionMultiplexer redis, Config config)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new SessionConverter() },
    };

    public async Task<Guid> SaveAsync(Session session, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();

        var sessionId = Guid.NewGuid();

        var sessionsKey = RedisKeys.Sessions(config.Redis, session.UserId);
        var sessionKey = RedisKeys.Session(config.Redis, sessionId);

        var score = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeMilliseconds();
        var encoded = JsonSerializer.Serialize(session, Options)!;

        await db.StringSetAsync(sessionKey, encoded, ttl);
        await db.SortedSetAddAsync(sessionsKey, sessionId.ToString(), score).WaitAsync(ct);

        return sessionId;
    }

    public async Task<Session?> GetAsync(Guid sessionId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();

        var sessionKey = RedisKeys.Session(config.Redis, sessionId);

        var value = await db.StringGetAsync(sessionKey).WaitAsync(ct);
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<Session>(value.ToString(), Options);
    }

    public async Task<List<Guid>> GetSessionIdsAsync(UserId userId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();

        var sessionsKey = RedisKeys.Sessions(config.Redis, userId);

        var values = await db.SortedSetRangeByScoreWithScoresAsync(sessionsKey).WaitAsync(ct);

        return values.Select(val => Guid.Parse(val.Element.ToString())).ToList();
    }

    public async Task RemoveAsync(UserId userId, Guid sessionId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();

        var sessionsKey = RedisKeys.Sessions(config.Redis, userId);
        var sessionKey = RedisKeys.Session(config.Redis, sessionId);

        await db.KeyDeleteAsync(sessionKey).WaitAsync(ct);
        await db.SortedSetRemoveAsync(sessionsKey, sessionId.ToString()).WaitAsync(ct);
    }

    public async Task RemoveAllAsync(UserId userId, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var sessionIds = await GetSessionIdsAsync(userId, ct);

        await Task.WhenAll(
            sessionIds.Select(async id =>
                await db.KeyDeleteAsync(RedisKeys.Session(config.Redis, id)).WaitAsync(ct)
            )
        );
        await db.KeyDeleteAsync(RedisKeys.Sessions(config.Redis, userId)).WaitAsync(ct);
    }

    public async Task RemoveExpiredAsync(CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints()[0]);

        var sessionsKeyPrefix = $"{config.Redis.KeyPrefix}{RedisPrefixes.Sessions}*";
        var sessionKeyPrefix = $"{config.Redis.KeyPrefix}{RedisPrefixes.Session}";

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await foreach (var key in server.KeysAsync(pattern: sessionsKeyPrefix, pageSize: 100))
        {
            ct.ThrowIfCancellationRequested();

            var sessions = await db.SortedSetRangeByScoreAsync(key, double.NegativeInfinity, now)
                .WaitAsync(ct);

            if (sessions.Length == 0)
            {
                continue;
            }

            await Task.WhenAll(
                sessions.Select(async session =>
                    await db.KeyDeleteAsync($"{sessionKeyPrefix}{session}").WaitAsync(ct)
                )
            );

            await db.SortedSetRemoveAsync(key, sessions).WaitAsync(ct);

            if (await db.SortedSetLengthAsync(key).WaitAsync(ct) == 0)
            {
                await db.KeyDeleteAsync(key).WaitAsync(ct);
            }
        }
    }
}
