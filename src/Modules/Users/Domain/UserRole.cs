using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record UserRole : NonEmptyString
{
    public static UserRole Regular => new("regular");
    public static UserRole Admin => new("admin");

    private UserRole(string value)
        : base(value) { }

    public static Result<UserRole> Create(string value) =>
        value?.ToLowerInvariant() switch
        {
            "regular" => Regular,
            "admin" => Admin,
            _ => Error.Validation("role", [$"Invalid user role: {value}"]),
        };

    public override string ToString() => base.ToString();
};
