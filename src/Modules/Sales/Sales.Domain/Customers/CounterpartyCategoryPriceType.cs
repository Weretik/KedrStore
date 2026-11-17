namespace Sales.Domain.Customers;

public sealed class CounterpartyCategoryPriceType
{
    public CounterpartyId CounterpartyId { get; private set; }
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
            CounterpartyId = CounterpartyId.Create(counterpartyId),
            CategoryId = categoryId,
            PriceTypeId = priceTypeId,
        };
    }
}
