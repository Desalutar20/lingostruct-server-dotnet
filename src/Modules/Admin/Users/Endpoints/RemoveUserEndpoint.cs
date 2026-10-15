using Microsoft.AspNetCore.Mvc;
using src.Modules.Admin.Users.UseCases.Commands.RemoveUser;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.Endpoints;

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder RemoveUserV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapDelete(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    [FromRoute] Guid id,
                    ICommandHandler<RemoveUserCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not Session
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("Admin.RemoveUser");

                    var command = new RemoveUserCommand(new UserId(id));
                    var result = await commandHandler.Handle(command, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(new ApiResponse<string>("Success"));
                }
            )
            .AddEndpointFilter<AuthenticationFilter>()
            .AddEndpointFilter(new AuthorizationFilter(UserRole.Admin))
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("RemoveUser")
            .WithSummary("Remove user")
            .WithDescription(
                "Permanently removes a user by their unique identifier. This action cannot be undone."
            );

        return endpoint;
    }
}
