using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Persistence.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => OrderId.FromStorage(value))
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property(order => order.OrderNumber)
            .HasMaxLength(32)
            .IsRequired();
        // Two indexes on one column need names: a unique one and the trigram one for search.
        builder.HasIndex(order => order.OrderNumber, "IX_Orders_OrderNumber").IsUnique();
        builder.HasIndex(order => order.OrderNumber, "IX_Orders_OrderNumber_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        // Database rules 2.2: a customer's orders and all orders are listed newest first; the first one also serves the
        // foreign key.
        builder.HasIndex(order => new { order.CounterpartyId, order.CreatedAt, order.Id })
            .HasDatabaseName("IX_Orders_CounterpartyId_CreatedAt_Id");
        builder.HasIndex(order => new { order.CreatedAt, order.Id })
            .HasDatabaseName("IX_Orders_CreatedAt_Id");

        builder.Property(order => order.CounterpartyId)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(order => order.Comment).HasMaxLength(1_000);

        // Platform spec 006, R-010 and R-011: the key of the order's 1C outbox message and the 1C answer.
        builder.Property(order => order.OneCMessageId).IsRequired();
        builder.HasIndex(order => order.OneCMessageId).IsUnique();
        builder.Property(order => order.OneCDocumentNumber).HasMaxLength(Order.OneCDocumentNumberMaxLength);
        builder.Property(order => order.OneCRejection).HasMaxLength(Order.OneCRejectionMaxLength);

        builder.HasOne<Counterparty>()
            .WithMany()
            .HasForeignKey(order => order.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Navigation(order => order.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
