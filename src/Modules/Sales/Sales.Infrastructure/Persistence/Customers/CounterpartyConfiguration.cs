namespace Sales.Infrastructure.Persistence.Customers;

public sealed class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable("Counterparties");

        builder.HasKey(counterparty => counterparty.Id);

        builder.Property(counterparty => counterparty.Id)
            .HasMaxLength(64)
            .ValueGeneratedNever();

        builder.Property(counterparty => counterparty.IdentityUserId)
            .IsRequired();

        builder.Property(counterparty => counterparty.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(counterparty => counterparty.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(counterparty => counterparty.Phone)
            .HasMaxLength(50);

        builder.Property(counterparty => counterparty.DefaultPriceTypeId)
            .IsRequired();

        builder.HasIndex(counterparty => counterparty.Email);
        // Database rules 2.1: the admin customer search runs ILIKE per word on these columns.
        builder.HasIndex(counterparty => counterparty.Id)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_Counterparties_Id_trgm");
        builder.HasIndex(counterparty => counterparty.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_Counterparties_Name_trgm");
        builder.HasIndex(counterparty => counterparty.Phone)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_Counterparties_Phone_trgm");
        builder.HasIndex(counterparty => counterparty.IdentityUserId)
            .IsUnique();
        builder.HasIndex(counterparty => counterparty.DefaultPriceTypeId);
        builder.HasQueryFilter(counterparty => !counterparty.IsDeleted);
    }
}
