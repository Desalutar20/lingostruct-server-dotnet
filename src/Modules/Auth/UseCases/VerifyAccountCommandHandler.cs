using src.Data;
using src.Modules.Auth.Services;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Data;

namespace src.Modules.Auth.UseCases;

internal sealed record VerifyAccountCommand(NonEmptyString Token, Email Email) : ICommand;

internal sealed class VerifyAccountCommandHandler(AppDbContext dbContext, AuthCache authCache)
    : ICommandHandler<VerifyAccountCommand>
{
    public async Task<Result> Handle(VerifyAccountCommand command, CancellationToken ct = default)
    {
        var userIdResult = await authCache.GetAndDeleteUserIdByVerificationTokenAsync(
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

        var user = await dbContext.Users.GetByIdAsync(userIdResult.Value, true, ct);
        if (user is null)
        {
            return AuthErrors.InvalidToken;
        }

        var verifyResult = user.Verify();
        if (verifyResult.IsFailure)
        {
            return verifyResult.Error;
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
