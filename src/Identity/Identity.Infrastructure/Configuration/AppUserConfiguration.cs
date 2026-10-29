using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Configuration;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AspNetUsers");
        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AvatarPath).HasMaxLength(200);
        builder
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

    }
}
