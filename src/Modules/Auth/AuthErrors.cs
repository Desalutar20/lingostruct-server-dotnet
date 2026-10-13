using src.Modules.Shared.Domain;

namespace src.Modules.Auth;

public static class AuthErrors
{
    public static Error InvalidToken => Error.Failure("Invalid or expired token");
    public static Error InvalidCredentials => Error.Failure("Invalid credentials");
    public static Error Unauthorized => Error.Unauthorized("Unauthorized");
    public static Error InvalidOAuthState => Error.Failure("Invalid or expired token");
}
