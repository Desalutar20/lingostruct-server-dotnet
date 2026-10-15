using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Domain.Pagination;

internal sealed record KeysetPagination<TId>(
    PositiveInt Limit,
    KeysetCursor<TId>? PrevCursor,
    KeysetCursor<TId>? NextCursor
)
    where TId : notnull
{
    public KeysetDirection KeysetDirection =>
        PrevCursor is not null ? KeysetDirection.Backward : KeysetDirection.Forward;

    public KeysetCursor<TId>? Cursor => PrevCursor ?? NextCursor;
}
