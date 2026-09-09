using Sales.Infrastructure.DataBase.Entities;

namespace Sales.Infrastructure.Configurations;

internal sealed class OrderSyncRetryAuditConfiguration : IEntityTypeConfiguration<OrderSyncRetryAudit>
{
    public void Configure(EntityTypeBuilder<OrderSyncRetryAudit> builder)
    {
        builder.ToTable("OrderSyncRetryAudits");

        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Id).ValueGeneratedOnAdd();
        builder.Property(audit => audit.OneCOrderSyncId)
            .HasConversion(id => id.Value, value => OneCOrderSyncId.FromStorage(value))
            .HasColumnType("bigint")
            .IsRequired();
        builder.Property(audit => audit.OrderId)
            .HasConversion(id => id.Value, value => OrderId.FromStorage(value))
            .HasColumnType("bigint")
            .IsRequired();
        builder.Property(audit => audit.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(audit => audit.PreviousStatus).HasMaxLength(32).IsRequired();
        builder.Property(audit => audit.PreviousErrorCode).HasMaxLength(128);
        builder.Property(audit => audit.PreviousErrorMessage).HasMaxLength(1_000);
        builder.Property(audit => audit.Reason).HasMaxLength(500).IsRequired();
        builder.Property(audit => audit.RequestedBy).HasMaxLength(255).IsRequired();

        builder.HasIndex(audit => audit.OrderId);
        builder.HasIndex(audit => audit.RequestedAtUtc);
        builder.HasOne<OneCOrderSync>()
            .WithMany()
            .HasForeignKey(audit => audit.OneCOrderSyncId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
