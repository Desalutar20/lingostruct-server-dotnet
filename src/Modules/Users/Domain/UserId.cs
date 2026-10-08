using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Users.Domain;

public sealed record UserId(Guid Value) : Id(Value);
