using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Configurations;

internal sealed class OrderDraftConfiguration : IEntityTypeConfiguration<OrderDraft>
{
    public void Configure(EntityTypeBuilder<OrderDraft> builder)
    {
        builder.ToTable("OrderDrafts");
        builder.HasKey(draft => draft.Id);
        builder.Property(draft => draft.Id)
            .HasConversion(id => id.Value, value => OrderDraftId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(draft => draft.CounterpartyId).HasMaxLength(64).IsRequired();
        builder.Property(draft => draft.Title).HasMaxLength(200).IsRequired();
        builder.Property(draft => draft.Note).HasMaxLength(2_000);
        builder.Property(draft => draft.Version).HasMaxLength(32).IsRequired().IsConcurrencyToken();
        builder.Property(draft => draft.CreatedAtUtc).IsRequired();
        builder.Property(draft => draft.UpdatedAtUtc).IsRequired();
        builder.HasIndex(draft => new { draft.CounterpartyId, draft.UpdatedAtUtc, draft.Id });
        builder.HasIndex(draft => new { draft.UpdatedAtUtc, draft.Id });
        builder.HasOne<Counterparty>().WithMany()
            .HasForeignKey(draft => draft.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict).IsRequired();
        builder.HasMany(draft => draft.Lines).WithOne()
            .HasForeignKey("DraftId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.Navigation(draft => draft.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}