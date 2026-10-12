using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.UseCases;

public sealed record LogoutCommand(UserId UserId, Guid SessionId) : ICommand;

internal sealed class LogoutCommandHandler(SessionStore sessionStore)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct = default)
    {
        await sessionStore.RemoveAsync(command.UserId, command.SessionId, ct);

        return Result.Success();
    }
}
