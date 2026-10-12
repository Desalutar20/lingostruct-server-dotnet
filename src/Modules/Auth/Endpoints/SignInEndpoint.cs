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
public sealed record SignInRequest(string Email, string Password);

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
        RuleFor(x => x.Password).ValidateValueObject(Password.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder SignInV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/sign-in",
                async (
                    HttpContext ctx,
                    SignInRequest request,
                    [FromServices] ICommandHandler<SignInCommand, (UserDto, Guid)> commandHandler,
                    Config.Config config,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.SignIn");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    SetSessionCookie(ctx, result.Value.Item2, config.Application);

                    return Results.Ok(new ApiResponse<UserDto>(result.Value.Item1));
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<UserDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("SignIn")
            .WithSummary("Authenticates a user")
            .WithDescription(
                "Logs in a user using email and password and returns authentication info"
            )
            .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }

    private static SignInCommand ToCommand(this SignInRequest request)
    {
        var email = Email.Create(request.Email).Value;
        var password = Password.Create(request.Password).Value;

        return new SignInCommand(email, password);
    }
}
