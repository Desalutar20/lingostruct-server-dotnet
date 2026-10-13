using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Outbox;

public sealed record OutboxType : NonEmptyString
{
    public static OutboxType Email => new("email");

    private OutboxType(string value)
        : base(value) { }

    public static Result<OutboxType> Create(string value) =>
        value?.ToLowerInvariant() switch
        {
            "email" => Email,
            _ => Error.Internal($"Invalid outbox type: {value}"),
        };

    public override string ToString() => base.ToString();
};
