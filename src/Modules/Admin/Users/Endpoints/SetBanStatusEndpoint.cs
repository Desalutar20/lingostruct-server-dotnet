using Microsoft.AspNetCore.Mvc;
using src.Modules.Admin.Users.UseCases.Commands.SetBanStatus;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.Endpoints;

public sealed record SetBanStatusRequest(bool BanStatus);

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder SetBanStatusV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}/ban",
                async (
                    HttpContext ctx,
                    Guid id,
                    SetBanStatusRequest request,
                    ICommandHandler<SetBanStatusCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.SetBanStatus");

                    var command = new SetBanStatusCommand(new UserId(id), request.BanStatus);
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
            .WithName("SetBanStatus")
            .WithSummary("Set user ban status")
            .WithDescription("Sets user's banned state .");

        return endpoint;
    }
}
