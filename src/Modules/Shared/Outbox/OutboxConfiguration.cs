using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Data;
using src.Modules.Shared.Domain.ValueObjects;

namespace src.Modules.Shared.Outbox;

internal sealed class OutboxConfiguration()
    : BaseConfiguration<Outbox, OutboxId>("outbox", val => new OutboxId(val))
{
    public override void Configure(EntityTypeBuilder<Outbox> builder)
    {
        base.Configure(builder);
        var types = string.Join(
            ", ",
            new[] { OutboxType.Email }.Select(r => $"'{r.Value.ToLowerInvariant()}'")
        );

        // builder.ToTable(
        //     "outbox",
        //     table => table.HasCheckConstraint("CK_type", $"type IN ({types})")
        // );

        // builder.HasKey(x => x.Id);

        // builder
        //     .Property(x => x.Id)
        //     .HasConversion(id => id.Value, value => new OutboxId(value))
        //     .ValueGeneratedNever();

        // builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        // builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnType("timestamptz");

        builder
            .Property(x => x.Data)
            .HasConversion(data => data.Value, value => NonEmptyString.Create(value).Value)
            .HasColumnType("jsonb")
            .IsRequired();

        builder
            .Property(x => x.Type)
            .HasConversion(role => role.Value.ToLower(), value => OutboxType.Create(value).Value)
            .IsRequired();
    }
}
