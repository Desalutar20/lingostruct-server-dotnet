using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.UseCases;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;

namespace src.Modules.Auth.Endpoints;

[Serializable]
public sealed record VerifyAccountRequest(string Token, string Email);

public sealed class VerifyAccountRequestValidator : AbstractValidator<VerifyAccountRequest>
{
    public VerifyAccountRequestValidator()
    {
        RuleFor(x => x.Token).ValidateValueObject(val => NonEmptyString.Create(val));
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder VerifyAccountV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/verify-account",
                async (
                    VerifyAccountRequest request,
                    [FromServices] ICommandHandler<VerifyAccountCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.VerifyAccount");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(
                            new ApiResponse<string>(
                                "Your account has been successfully verified. You can now log in."
                            )
                        );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("VerifyAccount")
            .WithSummary("Verifies a user's account")
            .WithDescription("Marks a user account as verified using the provided token and email.")
            .RequireRateLimiting(RateLimitConstants.VerifyAccount);

        return endpoint;
    }

    private static VerifyAccountCommand ToCommand(this VerifyAccountRequest request)
    {
        var token = NonEmptyString.Create(request.Token).Value;
        var email = Email.Create(request.Email).Value;

        return new VerifyAccountCommand(token, email);
    }
}
