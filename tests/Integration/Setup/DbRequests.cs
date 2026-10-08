using Microsoft.EntityFrameworkCore;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<User?> GetUserFromDbByEmail(Email email, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email, ct);
}
