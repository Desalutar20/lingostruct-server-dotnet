namespace src.Modules.Auth.Endpoints;

using src.Modules.Auth.UseCases.Commands.Logout;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder LogoutV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/logout",
                async (
                    HttpContext ctx,
                    ICommandHandler<LogoutCommand> commandHandler,
                    Config.Config config,
                    ILoggerFactory loggerFactory
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.Logout");

                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var userResult)
                        || !ctx.Items.TryGetValue(
                            RequestConstants.SessionKey,
                            out var sessionResult
                        )
                        || userResult is not Session session
                        || sessionResult is not Guid sessionId
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var command = new LogoutCommand(session.UserId, sessionId);
                    var result = await commandHandler.Handle(command);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    ClearSessionCookie(ctx, config.Application);

                    return Results.Ok(new ApiResponse<string>("success"));
                }
            )
            .AddEndpointFilter<AuthenticationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("Logout")
            .WithSummary("Logout current user")
            .WithDescription("Ends the current session and clears the authentication cookie.")
            .RequireRateLimiting(RateLimitConstants.Logout);

        return endpoint;
    }
}
