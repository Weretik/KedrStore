using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.QuickOrders;

namespace Sales.Infrastructure.Persistence.QuickOrders;

public sealed class QuickOrderConfiguration : IEntityTypeConfiguration<QuickOrder>
{
    /// <summary>The partial index of the «Лише з проблемою» filter (spec sales/009, R-006).</summary>
    public const string ProblemFilter = "\"TelegramSentAtUtc\" IS NULL AND (\"TelegramLastError\" IS NOT NULL OR \"TelegramFailedAtUtc\" IS NOT NULL)";

    public void Configure(EntityTypeBuilder<QuickOrder> builder)
    {
        builder.ToTable("QuickOrders");

        builder.HasKey(order => order.Id);
        // The site's Idempotency-Key is the id, so the primary key is the guard against a second copy (R-003).
        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => QuickOrderId.Create(value))
            .ValueGeneratedNever();

        builder.Property(order => order.FirstName).HasMaxLength(QuickOrder.FirstNameMaxLength).IsRequired();
        builder.Property(order => order.Phone).HasMaxLength(QuickOrder.PhoneMaxLength).IsRequired();
        builder.Property(order => order.PlacedAtUtc).IsRequired();
        builder.Property(order => order.Total).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.TelegramLastError).HasMaxLength(QuickOrder.TelegramErrorMaxLength);
        builder.Ignore(order => order.TelegramStatus);

        // Spec sales/009, R-010: the key of the 1C outbox message and the 1C order made from the site order. An order
        // removed with its customer leaves the site order without the link.
        builder.Property(order => order.OneCOrderMessageId);
        builder.Property(order => order.OneCOrderId)
            .HasConversion(id => id!.Value.Value, value => OrderId.FromStorage(value))
            .HasColumnType("bigint");
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(order => order.OneCOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // PostgreSQL xmin: two parallel resends or a resend racing a delivery make one change (database rules 4).
        builder.Property<uint>("xmin").IsRowVersion();

        // Database rules 2.1 and 2.2: the list is newest first; search is per word on name and phone; the problem
        // filter reads only the few undelivered rows.
        builder.HasIndex(order => new { order.PlacedAtUtc, order.Id })
            .HasDatabaseName("IX_QuickOrders_PlacedAtUtc_Id");
        builder.HasIndex(order => order.FirstName, "IX_QuickOrders_FirstName_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        builder.HasIndex(order => order.Phone, "IX_QuickOrders_Phone_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        builder.HasIndex(order => new { order.PlacedAtUtc, order.Id }, "IX_QuickOrders_Problem")
            .HasFilter(ProblemFilter);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey("QuickOrderId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
