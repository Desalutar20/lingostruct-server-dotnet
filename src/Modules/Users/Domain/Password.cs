using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record Password : NonEmptyString
{
    public const int MinLength = 8;
    public const int MaxLength = 60;

    private Password(string value)
        : base(value) { }

    public static Result<Password> Create(string value) =>
        NonEmptyString.Create<Password>(
            value,
            "password",
            "Password",
            MaxLength,
            (val) => new Password(val),
            MinLength
        );

    public override string ToString() => base.ToString();
};
