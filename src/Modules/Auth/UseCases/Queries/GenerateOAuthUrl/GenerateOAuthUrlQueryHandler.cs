namespace src.Modules.Auth.UseCases.Queries.GenerateOAuthUrl;

using src.Modules.Auth.OAuth;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

internal sealed record GenerateOAuthUrlQuery(OAuthProvider Provider, NonEmptyString? AdditionState)
    : IQuery<(Url, OAuthState)>;

internal sealed class GenerateOAuthUrlQueryHandler(OAuthClientFactory factory)
    : IQueryHandler<GenerateOAuthUrlQuery, (Url, OAuthState)>
{
    public async Task<Result<(Url, OAuthState)>> Handle(
        GenerateOAuthUrlQuery query,
        CancellationToken ct = default
    )
    {
        var provider = factory.Get(query.Provider);
        var state = new OAuthState(Guid.NewGuid(), query.AdditionState);

        var url = provider.GenerateRedirectUrl(state);

        return (url, state);
    }
}
