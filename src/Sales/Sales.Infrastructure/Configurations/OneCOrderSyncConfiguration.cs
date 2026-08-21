using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

public sealed class OneCOrderSyncConfiguration : IEntityTypeConfiguration<OneCOrderSync>
{
    public void Configure(EntityTypeBuilder<OneCOrderSync> builder)
    {
        builder.ToTable("OneCOrderSyncs", table =>
            table.HasCheckConstraint(
                "CK_OneCOrderSyncs_Status",
                "\"Status\" IN ('Pending', 'Sent', 'Accepted', 'BusinessError', 'TransportError', 'RetryScheduled', 'DeadLetter')"));

        builder.HasKey(sync => sync.Id);
        builder.Property(sync => sync.Id)
            .HasConversion(id => id.Value, value => OneCOrderSyncId.Create(value))
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property(sync => sync.OrderId)
            .HasConversion(id => id.Value, value => OrderId.Create(value))
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<OneCOrderSync>(sync => sync.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(sync => sync.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(sync => sync.LastErrorCode).HasMaxLength(128);
        builder.Property(sync => sync.LastErrorMessage).HasMaxLength(1_000);
        builder.Property(sync => sync.OneCRequestPayloadHash).HasMaxLength(128);
        builder.Property(sync => sync.OneCResponseBody).HasMaxLength(2_000);

        builder.HasIndex(sync => sync.OrderId).IsUnique();
        builder.HasIndex(sync => new { sync.Status, sync.NextAttemptAtUtc });

        // PostgreSQL xmin makes the worker claim (Pending/RetryScheduled -> Sent) optimistic and atomic.
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
