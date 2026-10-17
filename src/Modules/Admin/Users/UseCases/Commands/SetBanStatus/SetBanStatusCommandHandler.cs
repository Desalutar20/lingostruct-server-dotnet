using src.Data;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.UseCases.Commands.SetBanStatus;

internal sealed record SetBanStatusCommand(UserId UserId, bool BanStatus) : ICommand;

internal sealed class SetBanStatusCommandHandler(AppDbContext dbContext, SessionStore sessionStore)
    : ICommandHandler<SetBanStatusCommand>
{
    public async Task<Result> Handle(SetBanStatusCommand command, CancellationToken ct = default)
    {
        var user = await dbContext.Users.GetByIdAsync(command.UserId, true, ct);
        if (user is null)
        {
            return AdminUserErrors.UserNotFound(command.UserId);
        }

        var isUpdated = user.Update(
            Optional<FirstName?>.None,
            Optional<LastName?>.None,
            Optional<Url?>.None,
            Optional<bool>.Some(command.BanStatus)
        );

        if (!isUpdated)
        {
            dbContext.ChangeTracker.Clear();
            return Result.Success();
        }

        if (command.BanStatus)
        {
            await sessionStore.RemoveAllAsync(command.UserId, ct);
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
