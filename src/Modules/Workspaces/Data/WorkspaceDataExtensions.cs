namespace src.Modules.Workspaces.Data;

using Microsoft.EntityFrameworkCore;
using src.Modules.Workspaces.Domain;

internal static class WorkspaceDataExtensions
{
    public static Task<Workspace?> GetByIdAsync(
        this DbSet<Workspace> users,
        WorkspaceId id,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = users.Where(u => u.Id == id);
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(ct);
    }
}
