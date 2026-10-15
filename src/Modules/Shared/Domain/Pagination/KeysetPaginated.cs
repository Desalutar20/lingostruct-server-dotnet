namespace src.Modules.Shared.Domain.Pagination;

internal sealed record KeysetPaginated<T, TId>
    where TId : notnull
{
    public IReadOnlyList<T> Data { get; }
    public KeysetCursor<TId>? PrevCursor { get; }
    public KeysetCursor<TId>? NextCursor { get; }

    public KeysetPaginated(
        List<T> data,
        KeysetCursor<TId>? prevCursor,
        KeysetCursor<TId>? nextCursor
    )
    {
        Data = data.AsReadOnly();
        PrevCursor = prevCursor;
        NextCursor = nextCursor;
    }

    public KeysetPaginated(
        List<T> data,
        KeysetPagination<TId> pagination,
        Func<T, TId> id,
        Func<T, DateTimeOffset> createdAt
    )
    {
        var direction = pagination.KeysetDirection;

        var hasMore = data.Count > pagination.Limit.Value;
        if (hasMore)
        {
            data.RemoveAt(data.Count - 1);
        }

        var prevCursor = direction switch
        {
            KeysetDirection.Forward when pagination.NextCursor is not null => new KeysetCursor<TId>(
                id(data[0]),
                createdAt(data[0])
            ),

            KeysetDirection.Backward when hasMore => new KeysetCursor<TId>(
                id(data[^1]),
                createdAt(data[^1])
            ),

            _ => null,
        };

        var nextCursor = direction switch
        {
            KeysetDirection.Forward when hasMore => new KeysetCursor<TId>(
                id(data[^1]),
                createdAt(data[^1])
            ),

            KeysetDirection.Backward when pagination.PrevCursor is not null =>
                new KeysetCursor<TId>(id(data[0]), createdAt(data[0])),

            _ => null,
        };

        if (direction == KeysetDirection.Backward)
        {
            data.Reverse();
        }

        Data = data.AsReadOnly();
        PrevCursor = prevCursor;
        NextCursor = nextCursor;
    }
}
