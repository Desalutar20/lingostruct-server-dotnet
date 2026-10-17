namespace src.Modules.Workspaces.Data;

using System.IO.Hashing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Data;
using src.Modules.Workspaces.Domain;

internal sealed class WorkspaceConfiguration()
    : BaseConfiguration<Workspace, WorkspaceId>("workspace", id => new WorkspaceId(id))
{
    public override void Configure(EntityTypeBuilder<Workspace> builder)
    {
        base.Configure(builder);

        builder
            .Property(x => x.Name)
            .HasConversion(name => name.Value, value => WorkspaceName.Create(value).Value)
            .HasMaxLength(WorkspaceName.MaxLength)
            .IsRequired();

        builder.ComplexProperty(
            x => x.Address,
            address =>
            {
                address.IsRequired();

                address
                    .Property(x => x.Country)
                    .HasMaxLength(WorkspaceAddress.CountryMaxLength)
                    .IsRequired()
                    .HasColumnName("country");

                address
                    .Property(x => x.City)
                    .HasMaxLength(WorkspaceAddress.CityMaxLength)
                    .IsRequired()
                    .HasColumnName("city");

                address
                    .Property(x => x.Street)
                    .HasMaxLength(WorkspaceAddress.StreetMaxLength)
                    .IsRequired()
                    .HasColumnName("street");

                address
                    .Property(x => x.StreetNumber)
                    .HasMaxLength(WorkspaceAddress.StreetNumberMaxLength)
                    .IsRequired()
                    .HasColumnName("street_number");

                address
                    .Property(x => x.PostalCode)
                    .HasMaxLength(WorkspaceAddress.PostalCodeMaxLength)
                    .IsRequired()
                    .HasColumnName("postal_code");
            }
        );
    }
}
