using Sales.Infrastructure.Persistence.Customers;
using Sales.Infrastructure.Persistence.OrderDrafts;
using Sales.Infrastructure.Persistence.Orders;
using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
namespace Sales.Infrastructure.Persistence.Orders.Idempotency;

internal sealed class OrderIdempotencyRecordConfiguration : IEntityTypeConfiguration<OrderIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<OrderIdempotencyRecord> builder)
    {
        builder.ToTable("OrderIdempotencyRecords");

        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedOnAdd();
        builder.Property(record => record.Operation).HasMaxLength(64).IsRequired();
        builder.Property(record => record.IdempotencyKey).HasMaxLength(64).IsRequired();
        builder.Property(record => record.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(record => record.OrderId)
            .HasConversion(id => id.Value, value => OrderId.Create(value))
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasIndex(record => new { record.Operation, record.IdempotencyKey }).IsUnique();
        builder.HasIndex(record => record.OrderId).IsUnique();

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<OrderIdempotencyRecord>(record => record.OrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
