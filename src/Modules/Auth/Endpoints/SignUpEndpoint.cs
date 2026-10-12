namespace src.Modules.Auth.Endpoints;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.UseCases;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;
using src.Modules.Users.Domain;

[Serializable]
public sealed record SignUpRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
        RuleFor(x => x.Password).ValidateValueObject(Password.Create);
        RuleFor(x => x.FirstName).ValidateValueObject(FirstName.Create);
        RuleFor(x => x.LastName).ValidateValueObject(LastName.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder SignUpV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/sign-up",
                async (
                    SignUpRequest request,
                    [FromServices] ICommandHandler<SignUpCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.SignUp");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Created(
                            "/sign-up",
                            new ApiResponse<string>(
                                "If the email is registered, instructions have been sent"
                            )
                        );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("SignUp")
            .WithSummary("Registers a new user")
            .WithDescription("Creates a user with email, password, first name and last name")
            .RequireRateLimiting(RateLimitConstants.SignUp);

        return endpoint;
    }

    private static SignUpCommand ToCommand(this SignUpRequest request)
    {
        var email = Email.Create(request.Email).Value;
        var password = Password.Create(request.Password).Value;
        var firstName = FirstName.Create(request.FirstName).Value;
        var lastName = LastName.Create(request.LastName).Value;

        return new SignUpCommand(email, password, firstName, lastName);
    }
}
