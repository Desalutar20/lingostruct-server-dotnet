using src.Config;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.Services;

internal static class AuthService
{
    public static async Task<(Session, Guid)> GenerateSession(
        User user,
        SessionStore sessionStore,
        ApplicationConfig config,
        CancellationToken ct = default
    )
    {
        var sessions = await sessionStore.GetSessionIdsAsync(user.Id, ct);
        if (sessions.Count >= config.MaxSessions)
        {
            await sessionStore.RemoveAsync(user.Id, sessions[0], ct);
        }

        var session = Session.FromUser(user);
        var sessionId = await sessionStore.SaveAsync(
            session,
            TimeSpan.FromMinutes(config.SessionTtlMinutes),
            ct
        );

        return (session, sessionId);
    }
}
