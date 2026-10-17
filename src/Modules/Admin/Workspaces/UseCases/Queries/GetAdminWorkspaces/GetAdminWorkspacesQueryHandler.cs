using System.Data;
using src.Modules.Admin.Workspaces.Data;
using src.Modules.Admin.Workspaces.Dto;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.Pagination;

namespace src.Modules.Admin.Workspaces.UseCases.Queries.GetAdminWorkspaces;

internal sealed record GetAdminWorkspacesQuery(
    WorkspacesFilters Filters,
    KeysetPagination<Guid> Pagination
) : IQuery<KeysetPaginated<AdminWorkspaceDto, Guid>>;

internal sealed class GetAdminWorkspacesQueryHandler(IDbConnection dbConnection)
    : IQueryHandler<GetAdminWorkspacesQuery, KeysetPaginated<AdminWorkspaceDto, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminWorkspaceDto, Guid>>> Handle(
        GetAdminWorkspacesQuery query,
        CancellationToken ct = default
    )
    {
        var data = await dbConnection.GetWorkspacesAsync(query.Filters, query.Pagination, ct);

        return data;
    }
}
