using Microsoft.EntityFrameworkCore;
using Npgsql;
using src.Modules.Shared.Outbox;
using src.Modules.Users.Data;
using src.Modules.Users.Domain;
using src.Modules.Users.Domain.Exceptions;

namespace src.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly Dictionary<string, Func<Exception, Exception>> DuplicateExceptions =
        new() { [UserDbConstants.UniqueIndex] = ex => new UserAlreadyExistsException(ex) };

    private static readonly Dictionary<string, Func<Exception, Exception>> ForeignKeyExceptions =
        new() { };

    public DbSet<User> Users { get; set; }
    public DbSet<Outbox> Outboxes { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await base.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is not PostgresException pex)
            {
                throw;
            }

            if (
                TryMapException(
                    pex,
                    DbConstants.UniqueViolationErrorCode,
                    DuplicateExceptions,
                    out var duplicate
                )
            )
            {
                throw duplicate!;
            }

            if (
                TryMapException(
                    pex,
                    DbConstants.ForeignKeyViolationErrorCode,
                    ForeignKeyExceptions,
                    out var foreignKey
                )
            )
            {
                throw foreignKey!;
            }

            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    private static bool TryMapException(
        PostgresException pex,
        string sqlState,
        Dictionary<string, Func<Exception, Exception>> map,
        out Exception? exception
    )
    {
        exception = null;

        if (
            pex.SqlState != sqlState
            || pex.ConstraintName is null
            || !map.TryGetValue(pex.ConstraintName, out var factory)
        )
        {
            return false;
        }

        exception = factory(pex);
        return true;
    }
}
