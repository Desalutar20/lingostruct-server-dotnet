using Microsoft.EntityFrameworkCore;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;
using src.Modules.Workspaces.Domain;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<User?> GetUserFromDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email, ct);

    protected async Task<User?> GetUserFromDbById(UserId id, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, ct);

    protected async Task DeleteUserFromDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext.Users.Where(e => e.Email == email).ExecuteDeleteAsync(ct);

    protected async Task BanUserInDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext
            .Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsBanned, true), ct);

    protected async Task<Workspace?> GetWorkspaceFromDbById(WorkspaceId id, CancellationToken ct) =>
        await _dbContext.Workspaces.AsNoTracking().SingleOrDefaultAsync(w => w.Id == id, ct);
}
