using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.OAuth;
using src.Modules.Auth.UseCases.Commands.OAuthSignIn;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder OAuthSignInV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/{provider}/callback",
                async (
                    HttpContext ctx,
                    Config.Config config,
                    ILoggerFactory loggerFactory,
                    ICommandHandler<OAuthSignInCommand, Guid> commandHandler,
                    [FromRoute] string provider,
                    [FromQuery] string code,
                    [FromQuery] string state,
                    CancellationToken ct = default
                ) =>
                {
                    var oAuthStateCookieName = config.Application.OAuthStateCookieName;
                    if (
                        !ctx.Request.Cookies.TryGetValue(
                            oAuthStateCookieName,
                            out var expectedState
                        )
                    )
                    {
                        return TypedResults.Problem(
                            "Invalid state",
                            statusCode: StatusCodes.Status400BadRequest
                        );
                    }

                    var logger = loggerFactory.CreateLogger("Auth.OAuthSignIn");

                    var commandResult = ToCommand(state, code, provider, expectedState);
                    if (commandResult.IsFailure)
                    {
                        return commandResult.Error.ToApiError(logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    SetSessionCookie(ctx, result.Value, config.Application);

                    var redirectPath = commandResult.Value.Expected.AdditionalState?.Value ?? "";

                    return Results.Redirect($"{config.Application.ClientUrl}{redirectPath}");
                }
            )
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("OAuthSignIn")
            .WithSummary("OAuth sign-in callback")
            .WithDescription(
                "Handles OAuth provider callback and completes user authentication flow."
            )
            .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }

    private static Result<OAuthSignInCommand> ToCommand(
        string state,
        string code,
        string provider,
        string expectedState
    )
    {
        var providerResult = OAuthProvider.Create(provider);
        if (providerResult.IsFailure)
        {
            return providerResult.Error;
        }

        var codeResult = NonEmptyString.Create(code);
        if (codeResult.IsFailure)
        {
            return codeResult.Error;
        }

        var receivedStateResult = OAuthState.Create(state);
        if (receivedStateResult.IsFailure)
        {
            return receivedStateResult.Error;
        }

        var expectedStateResult = OAuthState.Create(expectedState);
        if (expectedStateResult.IsFailure)
        {
            return expectedStateResult.Error;
        }

        return new OAuthSignInCommand(
            providerResult.Value,
            codeResult.Value,
            receivedStateResult.Value,
            expectedStateResult.Value
        );
    }
}
