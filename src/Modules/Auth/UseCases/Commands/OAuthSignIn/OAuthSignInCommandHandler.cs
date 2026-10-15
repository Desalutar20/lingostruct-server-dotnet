namespace src.Modules.Auth.UseCases.Commands.OAuthSignIn;

using src.Data;
using src.Modules.Auth.OAuth;
using src.Modules.Auth.Services;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;

internal sealed record OAuthSignInCommand(
    OAuthProvider Provider,
    NonEmptyString Code,
    OAuthState Received,
    OAuthState Expected
) : ICommand<Guid>;

internal sealed class OAuthSignInCommandHandler(
    OAuthClientFactory clientFactory,
    AppDbContext dbContext,
    SessionStore sessionStore,
    Config.Config config
) : ICommandHandler<OAuthSignInCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        OAuthSignInCommand command,
        CancellationToken ct = default
    )
    {
        var client = clientFactory.Get(command.Provider);
        if (!client.IsValidState(command.Received, command.Expected))
        {
            return AuthErrors.InvalidOAuthState;
        }

        var oauthUser = await client.GetUserAsync(command.Code, ct);
        if (oauthUser.IsFailure)
        {
            return oauthUser.Error;
        }

        var user = await dbContext.Users.GetByEmailAsync(oauthUser.Value.Email, true, ct);
        if (user is null)
        {
            user = new User(oauthUser.Value.Email, null, null, null);

            dbContext.Users.Add(user);
        }

        var result = user.LinkOAuthProvider(command.Provider, oauthUser.Value.ProviderId);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await dbContext.SaveChangesAsync(ct);
        var (_, sessionId) = await AuthService.GenerateSession(
            user,
            sessionStore,
            config.Application,
            ct
        );

        return sessionId;
    }
}
