using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

internal sealed class OrderDraftLineConfiguration : IEntityTypeConfiguration<OrderDraftLine>
{
    public void Configure(EntityTypeBuilder<OrderDraftLine> builder)
    {
        builder.ToTable("OrderDraftLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id)
            .HasConversion(id => id.Value, value => OrderDraftLineId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property<OrderDraftId>("DraftId")
            .HasConversion(id => id.Value, value => OrderDraftId.FromStorage(value))
            .HasColumnType("bigint");
        builder.Property(line => line.ProductId).HasMaxLength(64).IsRequired();
        builder.Property(line => line.Quantity).IsRequired();
        builder.HasIndex("DraftId", nameof(OrderDraftLine.ProductId)).IsUnique();
        builder.ToTable(table => table.HasCheckConstraint("CK_OrderDraftLines_Quantity", "\"Quantity\" > 0"));
    }
}