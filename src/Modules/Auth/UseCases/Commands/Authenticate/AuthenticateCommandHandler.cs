namespace src.Modules.Auth.UseCases.Commands.Authenticate;

using src.Modules.Auth;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;

internal sealed record AuthenticateCommand(Guid SessionId) : ICommand<Session>;

internal sealed class AuthenticateCommandHandler(SessionStore sessionStore)
    : ICommandHandler<AuthenticateCommand, Session>
{
    public async Task<Result<Session>> Handle(
        AuthenticateCommand command,
        CancellationToken ct = default
    )
    {
        var session = await sessionStore.GetAsync(command.SessionId, ct);
        if (session is null)
        {
            return AuthErrors.Unauthorized;
        }

        return session;
    }
}
