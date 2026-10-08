using src.Modules.Shared.Constants;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using StackExchange.Redis;

namespace src.Modules.Auth;

public sealed class AuthCache(IConnectionMultiplexer redis, Config.Config config)
{
    public async Task StoreVerificationTokenAsync(
        UserId userId,
        NonEmptyString token,
        CancellationToken ct = default
    ) =>
        await StoreToken(
            TokenType.AccountVerification,
            userId,
            token,
            TimeSpan.FromMinutes(config.Application.AccountVerificationTtlMinutes),
            ct
        );

    private async Task StoreToken(
        TokenType tokenType,
        UserId userId,
        NonEmptyString token,
        TimeSpan ttl,
        CancellationToken ct
    )
    {
        var db = redis.GetDatabase();

        var cacheKey = tokenType switch
        {
            TokenType.AccountVerification => RedisKeys.AccountVerification(config.Redis, token),
            TokenType.ResetPassword => RedisKeys.PasswordReset(config.Redis, token),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null),
        };

        await db.StringSetAsync(cacheKey, userId.Value.ToString(), ttl).WaitAsync(ct);
    }

    private enum TokenType
    {
        AccountVerification,
        ResetPassword,
    }
}
