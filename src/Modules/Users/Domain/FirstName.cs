using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record FirstName : NonEmptyString
{
    public const int MaxLength = 30;

    private FirstName(string value)
        : base(value) { }

    public static Result<FirstName> Create(string value) =>
        NonEmptyString.Create<FirstName>(
            value,
            "firstName",
            "First name",
            MaxLength,
            (val) => new FirstName(val)
        );
};
