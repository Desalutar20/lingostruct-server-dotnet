using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Auth.OAuth;

internal interface IOAuthClient
{
    Url GenerateRedirectUrl(OAuthState state);
    bool IsValidState(OAuthState received, OAuthState expected);
    Task<Result<OAuthUser>> GetUserAsync(NonEmptyString code, CancellationToken ct = default);
}
