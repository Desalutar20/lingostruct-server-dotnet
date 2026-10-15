using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.OAuth;
using src.Modules.Auth.UseCases.Queries.GenerateOAuthUrl;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder GetOauthRedirectUrlV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/{provider}",
                async (
                    HttpContext ctx,
                    Config.Config config,
                    IQueryHandler<GenerateOAuthUrlQuery, (Url, OAuthState)> queryHandler,
                    [FromRoute] string provider,
                    [FromQuery] string? redirectPath,
                    ILoggerFactory loggerFactory
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.GetOauthRedirectUrl");
                    var providerResult = OAuthProvider.Create(provider);
                    if (providerResult.IsFailure)
                    {
                        return providerResult.Error.ToApiError(logger);
                    }

                    NonEmptyString? additionalState = null;
                    if (redirectPath is not null)
                    {
                        var additionalStateResult = NonEmptyString.Create(redirectPath);
                        if (additionalStateResult.IsFailure)
                        {
                            return additionalStateResult.Error.ToApiError(logger);
                        }

                        additionalState = additionalStateResult.Value;
                    }

                    var query = new GenerateOAuthUrlQuery(providerResult.Value, additionalState);
                    var result = await queryHandler.Handle(query);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(
                            config.Application.OAuthStateTtlMinutes
                        ),
                        HttpOnly = true,
                        Secure = config.Application.CookieSecure,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                    };

                    ctx.Response.Cookies.Append(
                        config.Application.OAuthStateCookieName,
                        result.Value.Item2.ToString(),
                        cookieOptions
                    );

                    return Results.Redirect(result.Value.Item1.ToString());
                }
            )
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetOauthRedirectUrl")
            .WithSummary("Generates OAuth redirect URL")
            .WithDescription("Returns a provider-specific OAuth redirect URL.")
            .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }
}
