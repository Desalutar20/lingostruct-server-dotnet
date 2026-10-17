using System.Data;
using Dapper;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Admin.Workspaces.UseCases.Queries.GetAdminWorkspaces;
using src.Modules.Shared.Domain.Pagination;
using src.Modules.Shared.Extensions;

namespace src.Modules.Admin.Workspaces.Data;

internal static class AdminWorkspacesDataExtensions
{
    public static async Task<KeysetPaginated<AdminWorkspaceDto, Guid>> GetWorkspacesAsync(
        this IDbConnection connection,
        WorkspacesFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        var search = filters.Search?.Value.ToLower();

        builder.WhereIf(search is not null, "w.name ILIKE @Search", new { Search = $"{search}%" });

        builder.ApplyKeysetPagination(keysetPagination, "w");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       name as Name,
                       country as Country,
                       city as City,
                       street as Street,
                       street_number as StreetNumber,
                       postal_code as PostalCode
                FROM workspace w
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminWorkspaceDto>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminWorkspaceDto, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.Id,
            response => response.CreatedAt
        );
    }
}
