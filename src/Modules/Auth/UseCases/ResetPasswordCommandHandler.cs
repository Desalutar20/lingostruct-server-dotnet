using src.Data;
using src.Modules.Auth;
using src.Modules.Auth.Services;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Services;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;

internal sealed record ResetPasswordCommand(NonEmptyString Token, Email Email, Password NewPassword)
    : ICommand;

internal sealed class ResetPasswordCommandHandler(
    AppDbContext dbContext,
    SessionStore sessionStore,
    AuthCache cache
) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct = default)
    {
        var userIdResult = await cache.GetAndDeleteUserIdByResetPasswordTokenAsync(
            command.Token,
            ct
        );
        if (userIdResult is null)
        {
            return AuthErrors.InvalidToken;
        }

        if (userIdResult.IsFailure)
        {
            return userIdResult.Error;
        }

        var user = await dbContext.Users.GetById(userIdResult.Value, true, ct);
        if (user is null || !user.IsValid || user.Email != command.Email)
        {
            return AuthErrors.InvalidToken;
        }

        var hashedPasswordResult = HashingService.Hash(command.NewPassword);
        if (hashedPasswordResult.IsFailure)
        {
            return hashedPasswordResult.Error;
        }

        user.UpdatePassword(hashedPasswordResult.Value);

        await dbContext.SaveChangesAsync(ct);
        await sessionStore.RemoveAllAsync(user.Id, ct);

        return Result.Success();
    }
}
