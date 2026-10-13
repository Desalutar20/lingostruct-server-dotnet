using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record LastName : NonEmptyString
{
    public const int MaxLength = 50;

    private LastName(string value)
        : base(value) { }

    public static Result<LastName> Create(string value) =>
        NonEmptyString.Create<LastName>(
            value,
            "lastName",
            "Last name",
            MaxLength,
            (val) => new LastName(val)
        );

    public override string ToString() => base.ToString();
};
