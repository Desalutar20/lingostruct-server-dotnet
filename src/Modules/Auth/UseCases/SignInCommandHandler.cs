using src.Data;
using src.Modules.Auth.Services;
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
        var user = await dbContext.Users.GetByEmailAsync(command.Email, false, ct);
        if (
            user is null
            || user.HashedPassword == null
            || !HashingService.Verify(command.Password, user.HashedPassword)
            || !user.IsValid
        )
        {
            return AuthErrors.InvalidCredentials;
        }

        var (session, sessionId) = await AuthService.GenerateSession(
            user,
            sessionStore,
            config.Application,
            ct
        );

        return (session.ToDto(), sessionId);
    }
}
