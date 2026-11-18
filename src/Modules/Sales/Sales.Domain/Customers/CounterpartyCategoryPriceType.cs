namespace Sales.Domain.Customers;

public sealed class CounterpartyCategoryPriceType
{
    public CounterpartyId CounterpartyId { get; private set; }
    public ProductCategoryId CategoryId { get; private set; }
    public PriceTypeId PriceTypeId { get; private set; }

    private CounterpartyCategoryPriceType() { }

    public static DomainResult<CounterpartyCategoryPriceType> Create(
        string counterpartyId,
        ProductCategoryId categoryId,
        PriceTypeId priceTypeId)
    {
        if (string.IsNullOrWhiteSpace(counterpartyId))
            return CounterpartyErrors.IdRequired();
        if (categoryId.Value <= 0)
            return CounterpartyErrors.CategoryInvalid(categoryId.Value);
        if (priceTypeId.Value <= 0)
            return CounterpartyErrors.PriceTypeInvalid(priceTypeId.Value);

        return new CounterpartyCategoryPriceType
        {
            CounterpartyId = CounterpartyId.Create(counterpartyId),
            CategoryId = categoryId,
            PriceTypeId = priceTypeId,
        };
    }
}
