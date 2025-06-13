namespace Infrastructure.Identity.Configuration;

public class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
{
    public void Configure(EntityTypeBuilder<AppUserRole> builder)
    {
        builder.ToTable("AspNetUserRoles");

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder
            .HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId);

        builder
            .HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId);

        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.AssignedByUserId).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(300).IsRequired(false);
        builder.Property(x => x.IsTemporary).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ExpiresAt).IsRequired(false);
    }
}
