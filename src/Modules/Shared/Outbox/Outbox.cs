using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Outbox;

public sealed class Outbox(OutboxType type, NonEmptyString data)
    : Entity<OutboxId>(new OutboxId(Guid.NewGuid()))
{
    public OutboxType Type { get; private set; } = type;
    public NonEmptyString Data { get; private set; } = data;
    public DateTimeOffset? ProcessedAt { get; private set; }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
