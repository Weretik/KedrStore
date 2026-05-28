namespace Sales.Infrastructure.Configurations;

public sealed class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable("Counterparties");

        builder.HasKey(counterparty => counterparty.Id);

        builder.Property(counterparty => counterparty.Id)
            .HasMaxLength(64)
            .ValueGeneratedNever();

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
        builder.HasIndex(counterparty => counterparty.DefaultPriceTypeId);
        builder.HasQueryFilter(counterparty => !counterparty.IsDeleted);
    }
}
