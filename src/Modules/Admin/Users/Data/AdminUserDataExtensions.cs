using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using src.Modules.Admin.Users.Dto;
using src.Modules.Shared.Domain.Pagination;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Extensions;
using src.Modules.Users.Domain;

namespace src.Modules.Admin.Users.Data;

internal static class AdminUserDataExtensions
{
    public static async Task<KeysetPaginated<AdminUserDto, Guid>> GetUsersAsync(
        this IDbConnection connection,
        UsersFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        var search = filters.Search?.Value.ToLower();

        builder.WhereIf(
            search is not null,
            "(u.email ILIKE @Search OR u.first_name ILIKE @Search OR u.last_name ILIKE @Search)",
            new { Search = $"{search}%" }
        );

        builder.WhereIf(
            filters.IsVerified is not null,
            "u.is_verified = @IsVerified",
            new { filters.IsVerified }
        );

        builder.WhereIf(
            filters.IsBanned is not null,
            "u.is_banned = @IsBanned",
            new { filters.IsBanned }
        );

        builder.ApplyKeysetPagination(keysetPagination, "u");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       email as Email,
                       first_name as FirstName,
                       last_name as LastName,
                       role as Role,
                       avatar_url as AvatarUrl,
                       is_verified as IsVerified,
                       is_banned as IsBanned,
                       google_id as GoogleId,
                       github_Id as GithubId
                FROM users u
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminUserDto>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminUserDto, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.Id,
            response => response.CreatedAt
        );
    }
}
