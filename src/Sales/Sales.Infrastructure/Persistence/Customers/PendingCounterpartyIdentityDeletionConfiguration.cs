using Sales.Infrastructure.Persistence.Customers;
using Sales.Infrastructure.Persistence.OrderDrafts;
using Sales.Infrastructure.Persistence.Orders;

namespace Sales.Infrastructure.Persistence.Customers;

internal sealed class PendingCounterpartyIdentityDeletionConfiguration
    : IEntityTypeConfiguration<PendingCounterpartyIdentityDeletion>
{
    public void Configure(EntityTypeBuilder<PendingCounterpartyIdentityDeletion> builder)
    {
        builder.ToTable("PendingCounterpartyIdentityDeletions");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.CounterpartyId).HasMaxLength(64);
        builder.Property(item => item.IdentityUserId).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.Property(item => item.LastError).HasMaxLength(500);
        builder.HasIndex(item => item.IdentityUserId);
        builder.HasIndex(item => new { item.CounterpartyId, item.IdentityUserId }).IsUnique();
    }
}
