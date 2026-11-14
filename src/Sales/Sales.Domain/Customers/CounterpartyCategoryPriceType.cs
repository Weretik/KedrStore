namespace Sales.Domain.Customers;

public sealed class CounterpartyCategoryPriceType
{
    public string CounterpartyId { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public int PriceTypeId { get; private set; }

    private CounterpartyCategoryPriceType() { }

    public static DomainResult<CounterpartyCategoryPriceType> Create(
        string counterpartyId,
        int categoryId,
        int priceTypeId)
    {
        if (string.IsNullOrWhiteSpace(counterpartyId))
            return CounterpartyErrors.IdRequired();
        if (categoryId <= 0)
            return CounterpartyErrors.CategoryInvalid(categoryId);
        if (priceTypeId <= 0)
            return CounterpartyErrors.PriceTypeInvalid(priceTypeId);

        return new CounterpartyCategoryPriceType
        {
            CounterpartyId = counterpartyId.Trim(),
            CategoryId = categoryId,
            PriceTypeId = priceTypeId,
        };
    }
}
