namespace src.Modules.Auth.Endpoints;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Shared.Endpoints.Dto;
using src.Modules.Shared.Endpoints.Extensions;
using src.Modules.Shared.Endpoints.Filters;
using src.Modules.Users.Domain;

[Serializable]
public sealed record ResetPasswordRequest(string Token, string Email, string NewPassword);

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).ValidateValueObject(val => NonEmptyString.Create(val));
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
        RuleFor(x => x.NewPassword).ValidateValueObject(Password.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder ResetPasswordV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/reset-password",
                async (
                    ResetPasswordRequest request,
                    [FromServices] ICommandHandler<ResetPasswordCommand> commandHandler,
                    Config.Config config,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.ResetPassword");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiResponse<string>("Your password has been reset successfully.")
                    );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ResetPassword")
            .WithSummary("Resets a user's password using a valid token.")
            .WithDescription(
                "Allows a user to set a new password if they provide a valid reset token and their email."
            )
            .RequireRateLimiting(RateLimitConstants.ResetPassword);

        return endpoint;
    }

    private static ResetPasswordCommand ToCommand(this ResetPasswordRequest request)
    {
        var token = NonEmptyString.Create(request.Token).Value;
        var email = Email.Create(request.Email).Value;
        var newPassword = Password.Create(request.NewPassword).Value;

        return new ResetPasswordCommand(token, email, newPassword);
    }
}
