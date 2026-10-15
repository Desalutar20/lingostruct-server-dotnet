using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;

namespace src.Modules.Auth.UseCases.Commands.RemoveExpiredSessions;

internal sealed record RemoveExpiredSessionCommand : ICommand;

internal sealed class RemoveExpiredSessionsCommandHandler(SessionStore sessionStore)
    : ICommandHandler<RemoveExpiredSessionCommand>
{
    public async Task<Result> Handle(
        RemoveExpiredSessionCommand command,
        CancellationToken ct = default
    )
    {
        await sessionStore.RemoveExpiredAsync(ct);
        return Result.Success();
    }
}
