using Identity.Infrastructure.Persistence.Roles;
using Identity.Infrastructure.Persistence.Sessions;
using Identity.Infrastructure.Persistence.Users;

namespace Identity.Infrastructure.Persistence.Users;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AspNetUsers");
        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AvatarPath).HasMaxLength(200);
        // Database rules 2.1: the staff candidate search runs ILIKE per word on these columns.
        builder.HasIndex(x => x.Email, "IX_AspNetUsers_Email_trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(x => x.FullName, "IX_AspNetUsers_FullName_trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

    }
}
