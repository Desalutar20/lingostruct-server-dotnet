using src.Data;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.UseCases.Commands.RemoveUser;

internal sealed record RemoveUserCommand(UserId UserId) : ICommand;

internal sealed class RemoveUserCommandHandler(AppDbContext dbContext, SessionStore sessionStore)
    : ICommandHandler<RemoveUserCommand>
{
    public async Task<Result> Handle(RemoveUserCommand command, CancellationToken ct = default)
    {
        var user = await dbContext.Users.GetByIdAsync(command.UserId, true, ct);
        if (user is null)
        {
            return AdminUserErrors.UserNotFound(command.UserId);
        }

        dbContext.Remove(user);

        await sessionStore.RemoveAllAsync(user.Id, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
