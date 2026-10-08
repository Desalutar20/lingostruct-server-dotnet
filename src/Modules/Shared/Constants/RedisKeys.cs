using src.Config;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Shared.Constants;

public static class RedisKeys
{
    public static string AccountVerification(RedisConfig config, NonEmptyString token) =>
        $"{config.KeyPrefix}{RedisPrefixes.AccountVerification}{token.Value}";

    public static string Session(RedisConfig config, string sessionId) =>
        $"{config.KeyPrefix}{RedisPrefixes.Session}{sessionId}";

    public static string Sessions(RedisConfig config, UserId userId) =>
        $"{config.KeyPrefix}{RedisPrefixes.Sessions}{userId.Value}";

    public static string PasswordReset(RedisConfig config, NonEmptyString token) =>
        $"{config.KeyPrefix}{RedisPrefixes.PasswordReset}{token.Value}";
}
