namespace src.Modules.Auth.Endpoints;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Auth.UseCases.Commands.ForgotPassword;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;

[Serializable]
public sealed record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder ForgotPasswordV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/forgot-password",
                async (
                    ForgotPasswordRequest request,
                    [FromServices] ICommandHandler<ForgotPasswordCommand> commandHandler,
                    Config.Config config,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.ForgotPassword");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiResponse<string>(
                            "If the email is registered, password reset instructions have been sent"
                        )
                    );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ForgotPassword")
            .WithSummary("Sends a password reset email to the user.")
            .WithDescription("Triggers the password reset workflow using the provided email.")
            .RequireRateLimiting(RateLimitConstants.ForgotPassword);

        return endpoint;
    }

    private static ForgotPasswordCommand ToCommand(this ForgotPasswordRequest request)
    {
        var email = Email.Create(request.Email).Value;

        return new ForgotPasswordCommand(email);
    }
}
