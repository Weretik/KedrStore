namespace Sales.Infrastructure.Configurations;

public sealed class CounterpartyCategoryPriceTypeConfiguration
    : IEntityTypeConfiguration<CounterpartyCategoryPriceType>
{
    public void Configure(EntityTypeBuilder<CounterpartyCategoryPriceType> builder)
    {
        builder.ToTable("CounterpartyCategoryPriceTypes");

        builder.HasKey(priceType => new { priceType.CounterpartyId, priceType.CategoryId });

        builder.Property(priceType => priceType.CounterpartyId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(priceType => priceType.CategoryId)
            .IsRequired();

        builder.Property(priceType => priceType.PriceTypeId)
            .IsRequired();

        builder.HasIndex(priceType => priceType.CategoryId);
        builder.HasIndex(priceType => priceType.PriceTypeId);
    }
}
