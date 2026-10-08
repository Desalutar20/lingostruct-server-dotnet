using src.Modules.Shared.Domain;

namespace src.Modules.Auth;

public static class AuthErrors
{
    public static Error InvalidToken => Error.Failure("Invalid or expired token");
}
