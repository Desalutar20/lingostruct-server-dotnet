using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record HashedPassword : NonEmptyString
{
    public const int MinLength = 40;
    public const int MaxLength = 200;

    private HashedPassword(string value)
        : base(value) { }

    public static Result<HashedPassword> Create(string value)
    {
        var result = NonEmptyString.Create<HashedPassword>(
            value,
            "hashedPassword",
            "Hashed password",
            MaxLength,
            (val) => new HashedPassword(val),
            MinLength
        );
        if (result.IsFailure)
        {
            return Error.Internal("Invalid hashed password");
        }

        return result.Value;
    }

    public override string ToString() => base.ToString();
};
