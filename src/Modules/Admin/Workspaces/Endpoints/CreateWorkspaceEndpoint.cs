using FluentValidation;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Admin.Workspaces.UseCases.Commands.CreateWorkspace;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;
using src.Modules.Users.Domain;
using src.Modules.Workspaces.Domain;

namespace src.Modules.Admin.Workspaces.Endpoints;

[Serializable]
public sealed record CreateWorkspaceRequest(
    string Name,
    string Country,
    string City,
    string Street,
    string StreetNumber,
    string PostalCode
);

public sealed class CreateWorkspaceRequestValidator : AbstractValidator<CreateWorkspaceRequest>
{
    public CreateWorkspaceRequestValidator()
    {
        RuleFor(x => x.Name).ValidateValueObject(WorkspaceName.Create);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(WorkspaceAddress.CountryMaxLength);
        RuleFor(x => x.City).NotEmpty().MaximumLength(WorkspaceAddress.CityMaxLength);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(WorkspaceAddress.StreetMaxLength);
        RuleFor(x => x.StreetNumber)
            .NotEmpty()
            .MaximumLength(WorkspaceAddress.StreetNumberMaxLength);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(WorkspaceAddress.PostalCodeMaxLength);
    }
}

internal static partial class AdminWorkspacesEndpoints
{
    private static IEndpointRouteBuilder CreateWorkspaceV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "",
                async (
                    CreateWorkspaceRequest request,
                    ICommandHandler<CreateWorkspaceCommand, AdminWorkspaceDto> commandHandler,
                    HttpContext ctx,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not Session session
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("CreateWorkspace");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Created("/", new ApiResponse<AdminWorkspaceDto>(result.Value));
                }
            )
            .AddEndpointFilter<AuthenticationFilter>()
            .AddEndpointFilter(new AuthorizationFilter(UserRole.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<Guid>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateWorkspace")
            .WithSummary("Create a workspace")
            .WithDescription(
                "Creates a new workspace with the specified name and address. Requires administrator permissions."
            );

        return endpoint;
    }

    private static CreateWorkspaceCommand ToCommand(this CreateWorkspaceRequest request)
    {
        var name = WorkspaceName.Create(request.Name).Value;
        var address = WorkspaceAddress
            .Create(
                request.Country,
                request.City,
                request.Street,
                request.StreetNumber,
                request.PostalCode
            )
            .Value;

        return new(name, address);
    }
}
