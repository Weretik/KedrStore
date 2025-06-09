using Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Identity.Configuration;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("AspNetRoles");

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.Scope)
            .HasMaxLength(100);

        builder.Property(x => x.AccessLevel)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.IsSystemRole)
            .HasDefaultValue(false);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId);

    }
}
