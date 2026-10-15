using Microsoft.EntityFrameworkCore;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Outbox;

internal static class OutboxDataExtensions
{
    public static async Task<IEnumerable<Outbox>> GetAndLockOutboxesForProcessingAsync(
        this DbSet<Outbox> outboxes,
        OutboxType type,
        PositiveInt batchSize,
        CancellationToken ct = default
    )
    {
        return await outboxes
            .FromSqlInterpolated(
                $"""
                    SELECT *
                    FROM outbox
                    WHERE processed_at IS NULL
                      AND type = {type.ToString().ToLower()}
                    ORDER BY created_at
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize.Value}
                """
            )
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public static async Task MarkAsProcessedAsync(
        this DbSet<Outbox> outboxes,
        IReadOnlyCollection<OutboxId> ids,
        DateTimeOffset processedAt,
        CancellationToken ct = default
    )
    {
        await outboxes
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.ProcessedAt, processedAt)
                        .SetProperty(x => x.UpdatedAt, processedAt),
                ct
            );
    }
}
