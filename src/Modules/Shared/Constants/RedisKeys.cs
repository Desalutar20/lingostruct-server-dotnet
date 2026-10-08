using src.Config;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Shared.Constants;

public static class RedisKeys
{
    public static string AccountVerification(RedisConfig config, NonEmptyString token) =>
        $"{config.KeyPrefix}account-verification:{token.Value}";

    public static string Session(RedisConfig config, string sessionId) =>
        $"{config.KeyPrefix}session:{sessionId}";

    public static string Sessions(RedisConfig config, UserId userId) =>
        $"{config.KeyPrefix}sessions:{userId.Value}";

    public static string PasswordReset(RedisConfig config, NonEmptyString token) =>
        $"{config.KeyPrefix}password-reset:{token.Value}";
}
