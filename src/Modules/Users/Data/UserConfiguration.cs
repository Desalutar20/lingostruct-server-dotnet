using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Users.Domain;

namespace src.Modules.Users.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var roles = string.Join(
            ", ",
            new[] { UserRole.Admin, UserRole.Regular }.Select(r =>
                $"'{r.Value.ToLowerInvariant()}'"
            )
        );

        builder.Ignore(x => x.IsValid);

        builder.ToTable(
            "users",
            table => table.HasCheckConstraint("CK_role", $"role IN ({roles})")
        );

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName(UserDbConstants.UniqueIndex);
        builder.HasIndex(x => x.GoogleId).IsUnique();
        builder.HasIndex(x => x.GithubId).IsUnique();

        builder
            .Property(x => x.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder
            .Property(x => x.FirstName)
            .HasConversion(
                firstName => firstName != null ? firstName.Value : null,
                value => value != null ? FirstName.Create(value).Value : null
            )
            .HasMaxLength(FirstName.MaxLength);

        builder
            .Property(x => x.LastName)
            .HasConversion(
                lastName => lastName != null ? lastName.Value : null,
                value => value != null ? LastName.Create(value).Value : null
            )
            .HasMaxLength(LastName.MaxLength);

        builder
            .Property(x => x.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder
            .Property(x => x.HashedPassword)
            .HasConversion(
                hashedPassword => hashedPassword != null ? hashedPassword.Value : null,
                value => value != null ? HashedPassword.Create(value).Value : null
            )
            .HasMaxLength(HashedPassword.MaxLength);

        builder
            .Property(x => x.Role)
            .HasConversion(role => role.Value.ToLower(), value => UserRole.Create(value).Value)
            .IsRequired()
            .HasDefaultValue(UserRole.Regular);

        builder
            .Property(x => x.GoogleId)
            .HasConversion(
                googleId => googleId != null ? googleId.Value : null,
                value => value != null ? ProviderId.Create(value).Value : null
            )
            .HasMaxLength(ProviderId.MaxLength);

        builder
            .Property(x => x.GithubId)
            .HasConversion(
                githubId => githubId != null ? githubId.Value : null,
                value => value != null ? ProviderId.Create(value).Value : null
            )
            .HasMaxLength(ProviderId.MaxLength);

        builder
            .Property(x => x.AvatarUrl)
            .HasConversion(
                avatarUrl => avatarUrl != null ? avatarUrl.Value : null,
                value => value != null ? Url.Create(value).Value : null
            )
            .HasMaxLength(ProviderId.MaxLength);

        builder.Property(x => x.IsVerified).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsBanned).IsRequired().HasDefaultValue(false);
    }
}
