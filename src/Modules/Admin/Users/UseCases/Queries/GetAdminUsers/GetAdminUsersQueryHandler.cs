using System.Data;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Modules.Admin.Users.Data;
using src.Modules.Admin.Users.Dto;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.Pagination;
using src.Modules.Shared.Extensions;
using src.Modules.Users.Domain;

internal sealed record GetAdminUsersQuery(UsersFilters Filters, KeysetPagination<Guid> Pagination)
    : IQuery<KeysetPaginated<AdminUserDto, Guid>>;

internal sealed class GetAdminUsersQueryHandler(IDbConnection dbConnection)
    : IQueryHandler<GetAdminUsersQuery, KeysetPaginated<AdminUserDto, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminUserDto, Guid>>> Handle(
        GetAdminUsersQuery query,
        CancellationToken ct = default
    )
    {
        var data = await dbConnection.GetUsersAsync(query.Filters, query.Pagination, ct);

        return data;
    }
}
