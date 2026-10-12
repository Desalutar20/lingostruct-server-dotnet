using src.Config;
using src.Data;
using src.Modules.Auth.Services;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Outbox;
using src.Modules.Shared.Services;
using src.Modules.Shared.Services.Email;
using src.Modules.Users.Data;

internal sealed record ForgotPasswordCommand(Email Email) : ICommand;

internal sealed class ForgotPasswordCommandHandler(
    AppDbContext dbContext,
    AuthCache cache,
    Config config
) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct = default)
    {
        var user = await dbContext.Users.GetByEmail(command.Email, false, ct);
        if (user is null || !user.IsValid)
        {
            return Result.Success();
        }

        var tokenResult = RandomTokenGenerator.Generate();
        if (tokenResult.IsFailure)
        {
            return tokenResult.Error;
        }

        var message = EmailMessageBuilder.BuildResetPasswordEmail(
            config.Application,
            user.Email,
            tokenResult.Value
        );

        var outbox = new Outbox(OutboxType.Email, EmailMessageBuilder.SerializeMessage(message));
        dbContext.Add(outbox);

        await cache.StoreUserIdByResetPasswordTokenAsync(user.Id, tokenResult.Value, ct);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
