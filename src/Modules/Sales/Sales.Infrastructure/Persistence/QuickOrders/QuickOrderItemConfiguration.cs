using Sales.Domain.QuickOrders;

namespace Sales.Infrastructure.Persistence.QuickOrders;

public sealed class QuickOrderItemConfiguration : IEntityTypeConfiguration<QuickOrderItem>
{
    public void Configure(EntityTypeBuilder<QuickOrderItem> builder)
    {
        builder.ToTable("QuickOrderItems");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => QuickOrderItemId.FromStorage(value))
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd();

        builder.Property<QuickOrderId>("QuickOrderId")
            .HasConversion(id => id.Value, value => QuickOrderId.Create(value));
        builder.Property(item => item.ProductId).HasMaxLength(QuickOrderItem.ProductIdMaxLength).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(QuickOrderItem.TitleMaxLength).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Quantity).IsRequired();
        builder.Ignore(item => item.Amount);
    }
}
