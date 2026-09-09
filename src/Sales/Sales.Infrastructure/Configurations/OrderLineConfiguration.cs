using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id)
            .HasConversion(id => id.Value, value => OrderLineId.FromStorage(value))
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property<OrderId>("OrderId")
            .HasConversion(id => id.Value, value => OrderId.FromStorage(value))
            .HasColumnType("bigint");
        builder.Property(line => line.ProductId).HasMaxLength(64).IsRequired();
        builder.Property(line => line.ProductName).HasMaxLength(512).IsRequired();
        builder.Property(line => line.Quantity).IsRequired();
        builder.Property(line => line.Amount).HasPrecision(18, 2).IsRequired();
    }
}
