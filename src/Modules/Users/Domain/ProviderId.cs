using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record ProviderId : NonEmptyString
{
    public const int MaxLength = 100;

    private ProviderId(string value)
        : base(value) { }

    public static Result<ProviderId> Create(string value) =>
        NonEmptyString.Create<ProviderId>(
            value,
            "providerId",
            "Provider ID",
            MaxLength,
            (val) => new ProviderId(val)
        );
};
