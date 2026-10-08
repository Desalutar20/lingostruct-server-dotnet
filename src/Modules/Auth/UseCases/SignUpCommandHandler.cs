using src.Data;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Outbox;
using src.Modules.Shared.Services;
using src.Modules.Shared.Services.Email;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;
using src.Modules.Users.Domain.Exceptions;

namespace src.Modules.Auth.UseCases;

public sealed record SignUpCommand(
    Email Email,
    Password Password,
    FirstName FirstName,
    LastName LastName
) : ICommand;

internal sealed class SignUpCommandHandler(
    AppDbContext dbContext,
    AuthCache authCache,
    Config.Config config
) : ICommandHandler<SignUpCommand>
{
    public async Task<Result> Handle(SignUpCommand command, CancellationToken ct = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var existingUser = await dbContext.Users.GetByEmail(command.Email, false, ct);
        if (existingUser is not null)
        {
            await Task.Delay(1000, ct);
            return Result.Success();
        }

        var hashedPasswordResult = HashingService.Hash(command.Password);
        if (hashedPasswordResult.IsFailure)
        {
            return hashedPasswordResult;
        }

        var tokenResult = RandomTokenGenerator.Generate();
        if (tokenResult.IsFailure)
        {
            return tokenResult;
        }

        var user = new User(
            command.FirstName,
            command.LastName,
            command.Email,
            hashedPasswordResult.Value
        );

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(ct);

            var message = EmailMessageBuilder.BuildAccountVerificationEmail(
                config.Application,
                command.Email,
                tokenResult.Value
            );

            var outbox = new Outbox(
                OutboxType.Email,
                EmailMessageBuilder.SerializeMessage(message)
            );

            await authCache.StoreVerificationTokenAsync(user.Id, tokenResult.Value, ct);

            dbContext.Outboxes.Add(outbox);

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success();
        }
        catch (UserAlreadyExistsException)
        {
            return Result.Success();
        }
    }
}
