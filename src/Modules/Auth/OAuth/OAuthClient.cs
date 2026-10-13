using src.Config;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Auth.OAuth;

internal abstract class OAuthClient(OAuthProviderConfig config) : IOAuthClient
{
    protected readonly string ClientId = config.ClientId;
    protected readonly string ClientSecret = config.ClientSecret;
    protected readonly string RedirectUrl = config.RedirectUrl;

    public abstract Url GenerateRedirectUrl(OAuthState state);
    public abstract Task<Result<OAuthUser>> GetUserAsync(
        NonEmptyString code,
        CancellationToken ct = default
    );

    public bool IsValidState(OAuthState received, OAuthState expected) => received == expected;
}
