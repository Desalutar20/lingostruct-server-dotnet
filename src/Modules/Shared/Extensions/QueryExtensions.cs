using System.Linq.Expressions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.Pagination;

namespace src.Modules.Shared.Extensions;

internal static class QueryExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate
    ) => condition ? query.Where(predicate) : query;

    public static IQueryable<T> ApplyKeysetPagination<T, TId>(
        this IQueryable<T> query,
        KeysetPagination<TId> pagination
    )
        where T : Entity<TId>
        where TId : notnull
    {
        var direction = pagination.KeysetDirection;
        var cursor = pagination.Cursor;

        if (cursor is not null)
        {
            query = query
                .WhereIf(
                    direction == KeysetDirection.Backward,
                    x =>
                        EF.Functions.GreaterThan(
                            ValueTuple.Create(x.CreatedAt, x.Id),
                            ValueTuple.Create(cursor.CreatedAt, cursor.Id)
                        )
                )
                .WhereIf(
                    direction == KeysetDirection.Forward,
                    x =>
                        EF.Functions.LessThan(
                            ValueTuple.Create(x.CreatedAt, x.Id),
                            ValueTuple.Create(cursor.CreatedAt, cursor.Id)
                        )
                );
        }

        query = direction switch
        {
            KeysetDirection.Backward => query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
            _ => query.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id),
        };

        return query.Take(pagination.Limit.Value + 1);
    }

    public static IQueryable<T> WhereNotNull<T, TProp>(
        this IQueryable<T> query,
        TProp? value,
        Expression<Func<T, bool>> predicate
    ) => value is null ? query : query.Where(predicate);

    public static SqlBuilder ApplyKeysetPagination<TId>(
        this SqlBuilder builder,
        KeysetPagination<TId> pagination,
        string alias
    )
        where TId : notnull
    {
        var direction = pagination.KeysetDirection;
        var cursor = pagination.Cursor;

        if (cursor != null)
        {
            builder.Where(
                direction == KeysetDirection.Forward
                    ? $@"
            {alias}.created_at < @CreatedAt
            OR ({alias}.created_at = @CreatedAt AND {alias}.id < @Id)
        "
                    : $@"
            {alias}.created_at > @CreatedAt
            OR ({alias}.created_at = @CreatedAt AND {alias}.id > @Id)
        ",
                new { cursor.CreatedAt, cursor.Id }
            );
        }

        return builder.OrderBy(
            direction == KeysetDirection.Backward
                ? $"{alias}.created_at ASC, {alias}.id ASC"
                : $"{alias}.created_at DESC, {alias}.id DESC"
        );
    }

    public static SqlBuilder WhereIf(
        this SqlBuilder builder,
        bool condition,
        string sql,
        dynamic? parameters = null
    ) => condition ? builder.Where(sql, parameters) : builder;
}
