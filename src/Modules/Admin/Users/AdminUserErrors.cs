using src.Modules.Shared.Domain;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users;

public static class AdminUserErrors
{
    public static Error UserNotFound(UserId id) =>
        Error.Failure($"User with id {id.Value} not found");
}
