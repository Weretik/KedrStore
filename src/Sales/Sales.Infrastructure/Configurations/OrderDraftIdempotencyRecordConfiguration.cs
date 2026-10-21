using Sales.Infrastructure.DataBase.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

internal sealed class OrderDraftIdempotencyRecordConfiguration : IEntityTypeConfiguration<OrderDraftIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<OrderDraftIdempotencyRecord> builder)
    {
        builder.ToTable("OrderDraftIdempotencyRecords");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedOnAdd();
        builder.Property(record => record.IdempotencyKey).HasMaxLength(64).IsRequired();
        builder.Property(record => record.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(record => record.DraftId)
            .HasConversion(id => id.Value, value => OrderDraftId.FromStorage(value))
            .HasColumnType("bigint").IsRequired();
        builder.HasIndex(record => record.IdempotencyKey).IsUnique();
        builder.HasOne<OrderDraft>().WithMany().HasForeignKey(record => record.DraftId)
            .OnDelete(DeleteBehavior.Cascade).IsRequired();
    }
}