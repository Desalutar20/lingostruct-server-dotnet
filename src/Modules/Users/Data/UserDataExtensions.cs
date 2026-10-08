using Microsoft.EntityFrameworkCore;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Users.Data;

public static class UserDataExtensions
{
    public static Task<User?> GetByEmail(
        this DbSet<User> users,
        Email email,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = users.Where(u => u.Email == email);
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(ct);
    }

    public static Task<User?> GetById(
        this DbSet<User> users,
        UserId id,
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
