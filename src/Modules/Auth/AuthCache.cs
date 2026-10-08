using src.Modules.Shared.Constants;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using StackExchange.Redis;

namespace src.Modules.Auth;

public sealed class AuthCache(IConnectionMultiplexer redis, Config.Config config)
{
    public async Task StoreUserIdByVerificationTokenAsync(
        UserId userId,
        NonEmptyString token,
        CancellationToken ct = default
    ) =>
        await StoreUserId(
            TokenType.AccountVerification,
            userId,
            token,
            TimeSpan.FromMinutes(config.Application.AccountVerificationTtlMinutes),
            ct
        );

    public async Task<Result<UserId>?> GetAndDeleteUserIdVerificationTokenAsync(
        NonEmptyString token,
        CancellationToken ct = default
    ) => await GetUserId(TokenType.AccountVerification, token, ct);

    private async Task<Result<UserId>?> GetUserId(
        TokenType tokenType,
        NonEmptyString token,
        CancellationToken ct = default
    )
    {
        var db = redis.GetDatabase();
        var key = tokenType switch
        {
            TokenType.AccountVerification => RedisKeys.AccountVerification(config.Redis, token),
            _ => RedisKeys.PasswordReset(config.Redis, token),
        };

        var value = await db.StringGetDeleteAsync(key).WaitAsync(ct);
        if (value.IsNull)
        {
            return null;
        }

        if (!Guid.TryParse(value.ToString(), out var guid))
        {
            return Error.Internal($"Invalid user id stored in Redis for token type '{tokenType}'.");
        }

        return new UserId(guid);
    }

    private async Task StoreUserId(
        TokenType tokenType,
        UserId userId,
        NonEmptyString token,
        TimeSpan ttl,
        CancellationToken ct
    )
    {
        var db = redis.GetDatabase();

        var key = tokenType switch
        {
            TokenType.AccountVerification => RedisKeys.AccountVerification(config.Redis, token),
            TokenType.ResetPassword => RedisKeys.PasswordReset(config.Redis, token),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null),
        };

        await db.StringSetAsync(key, userId.Value.ToString(), ttl).WaitAsync(ct);
    }

    private enum TokenType
    {
        AccountVerification,
        ResetPassword,
    }
}
