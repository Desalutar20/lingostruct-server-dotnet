using FluentValidation;
using src.Modules.Admin.Workspaces.Constants;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Admin.Workspaces.UseCases.Queries.GetAdminWorkspaces;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.Pagination;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;
using src.Modules.Shared.Endpoints.Validators;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Workspaces.Endpoints;

public sealed record GetAdminWorkspacesRequest(
    string? Search,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
) : KeysetPaginationRequest(Limit, PrevCursor, NextCursor);

public sealed class GetAdminWorkspacesRequestValidator
    : KeysetPaginationRequestValidator<GetAdminWorkspacesRequest>
{
    public GetAdminWorkspacesRequestValidator()
        : base(
            AdminWorkspacesConstants.GetAdminWorkspacesMaxLimit,
            AdminWorkspacesConstants.GetAdminWorkspacesCursorMaxLength
        )
    {
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, "search", "Search"))
            .MaximumLength(AdminWorkspacesConstants.GetAdminWorkspacesSearchMaxLength);
    }
}

internal static partial class AdminWorkspacesEndpoints
{
    private static IEndpointRouteBuilder GetAdminWorkspacesV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetAdminWorkspacesQuery,
                        KeysetPaginated<AdminWorkspaceDto, Guid>
                    > queryHandler,
                    [AsParameters] GetAdminWorkspacesRequest request,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var session)
                        || session is not Session
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("Admin.GetAdminWorkspaces");

                    var query = request.ToQuery();

                    var result = await queryHandler.Handle(query, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiKeysetPaginatedResponse<AdminWorkspaceDto>(
                            result.Value.Data,
                            result.Value.PrevCursor?.ToString(),
                            result.Value.NextCursor?.ToString()
                        )
                    );
                }
            )
            .AddEndpointFilter<AuthenticationFilter>()
            .AddEndpointFilter(new AuthorizationFilter(UserRole.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiKeysetPaginatedResponse<AdminWorkspaceDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetAdminWorkspaces")
            .WithSummary("Retrieves a paginated list of workspaces for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of workspaces. Supports search and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static GetAdminWorkspacesQuery ToQuery(this GetAdminWorkspacesRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? AdminWorkspacesConstants.GetAdminWorkspacesDefaultLimit)
            .Value;

        var search = request.Search is not null
            ? NonEmptyString.Create(request.Search).Value
            : null;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<Guid>
                .Create(
                    request.PrevCursor,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid user id") : id
                )
                .Value
            : null;

        var next = request.NextCursor is not null
            ? KeysetCursor<Guid>
                .Create(
                    request.NextCursor,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid user id") : id
                )
                .Value
            : null;

        var filters = new WorkspacesFilters(search);
        var pagination = new KeysetPagination<Guid>(limit, prev, next);

        return new(filters, pagination);
    }
}
