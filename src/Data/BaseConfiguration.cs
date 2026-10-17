namespace src.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Modules.Shared.Domain;
using src.Modules.Shared.Domain.ValueObjects;

internal class BaseConfiguration<T, TId>(string table, Func<Guid, TId> idConverter)
    : IEntityTypeConfiguration<T>
    where T : Entity<TId>
    where TId : Id
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(table);

        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => idConverter(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();
    }
}
