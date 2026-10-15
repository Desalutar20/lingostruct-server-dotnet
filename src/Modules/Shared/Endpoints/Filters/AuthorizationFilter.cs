using src.Modules.Shared.Endpoints.Constants;
using src.Modules.Users.Domain;

internal sealed class AuthorizationFilter(params UserRole[] roles) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var httpContext = context.HttpContext;

        if (
            !httpContext.Items.TryGetValue(RequestConstants.UserKey, out var result)
            || result is not Session user
        )
            return TypedResults.Problem(
                "Unauthorized",
                statusCode: StatusCodes.Status401Unauthorized
            );

        if (roles.Length > 0 && !roles.Contains(user.Role))
            return TypedResults.Problem(
                "Access denied",
                statusCode: StatusCodes.Status403Forbidden
            );

        return await next(context);
    }
}
