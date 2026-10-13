using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.OAuth;

internal sealed record OAuthUser(ProviderId ProviderId, Email Email);
