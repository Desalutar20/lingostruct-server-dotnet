namespace src.Modules.Admin.Users.Endpoints;

using FluentValidation;
using src.Modules.Admin.Users.Constants;
using src.Modules.Admin.Users.Dto;
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

public sealed record GetAdminUsersRequest(
    string? Search,
    bool? IsBanned,
    bool? IsVerified,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
) : KeysetPaginationRequest(Limit, PrevCursor, NextCursor);

public sealed class GetAdminUsersRequestValidator
    : KeysetPaginationRequestValidator<GetAdminUsersRequest>
{
    public GetAdminUsersRequestValidator()
        : base(
            AdminUsersConstants.GetAdminUsersMaxLimit,
            AdminUsersConstants.GetAdminUsersCursorMaxLength
        )
    {
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, "search", "Search"))
            .MaximumLength(AdminUsersConstants.GetAdminUsersSearchMaxLength);
    }
}

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder GetAdminUsersV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetAdminUsersQuery,
                        KeysetPaginated<AdminUserDto, Guid>
                    > queryHandler,
                    [AsParameters] GetAdminUsersRequest request,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetAdminUsers");

                    var query = request.ToQuery();

                    var result = await queryHandler.Handle(query, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiKeysetPaginatedResponse<AdminUserDto>(
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
            .Produces<ApiKeysetPaginatedResponse<AdminUserDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetAdminUsers")
            .WithSummary("Retrieves a paginated list of users for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of users. Supports search, ban status, verification status, and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static GetAdminUsersQuery ToQuery(this GetAdminUsersRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? AdminUsersConstants.GetAdminUsersDefaultLimit)
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

        var filters = new UsersFilters(search, request.IsBanned, request.IsVerified);
        var pagination = new KeysetPagination<Guid>(limit, prev, next);

        return new(filters, pagination);
    }
}
