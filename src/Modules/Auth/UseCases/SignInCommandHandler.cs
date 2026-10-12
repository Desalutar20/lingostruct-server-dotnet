using src.Data;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Services;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.UseCases;

internal sealed record SignInCommand(Email Email, Password Password) : ICommand<(UserDto, Guid)>;

internal sealed class SignInCommandHandler(
    AppDbContext dbContext,
    SessionStore sessionStore,
    Config.Config config
) : ICommandHandler<SignInCommand, (UserDto, Guid)>
{
    public async Task<Result<(UserDto, Guid)>> Handle(
        SignInCommand command,
        CancellationToken ct = default
    )
    {
        var user = await dbContext.Users.GetByEmail(command.Email, false, ct);
        if (
            user is null
            || user.HashedPassword == null
            || !HashingService.Verify(command.Password, user.HashedPassword)
            || !user.IsValid
        )
        {
            return AuthErrors.InvalidCredentials;
        }

        var sessions = await sessionStore.GetSessionIdsAsync(user.Id, ct);
        if (sessions.Count >= config.Application.MaxSessions)
        {
            await sessionStore.RemoveAsync(user.Id, sessions[0], ct);
        }

        var session = Session.FromUser(user);
        var sessionId = await sessionStore.SaveAsync(
            session,
            TimeSpan.FromMinutes(config.Application.SessionTtlMinutes),
            ct
        );

        return (session.ToDto(), sessionId);
    }
}
