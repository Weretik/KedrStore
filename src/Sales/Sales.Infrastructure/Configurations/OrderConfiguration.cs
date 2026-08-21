using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => OrderId.Create(value))
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property(order => order.OrderNumber)
            .HasMaxLength(32)
            .IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.Property(order => order.CounterpartyId)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(order => order.Comment).HasMaxLength(1_000);

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
