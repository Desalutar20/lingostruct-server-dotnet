using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Outbox;

public sealed record OutboxId(Guid Value) : Id(Value);
