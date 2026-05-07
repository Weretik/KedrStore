using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Configuration;

public class AppRefreshSessionConfiguration : IEntityTypeConfiguration<AppRefreshSession>
{
    public void Configure(EntityTypeBuilder<AppRefreshSession> builder)
    {
        builder.ToTable("AppRefreshSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.RevokedAtUtc });
        builder.HasIndex(x => x.AbsoluteExpiresAtUtc);

        builder.Property(x => x.CreatedByIp).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(256);
        builder.Property(x => x.RevocationReason).HasMaxLength(64);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
