namespace Sales.Domain.Customers.Entities;

public sealed class CounterpartyCategoryPriceType
{
    public string CounterpartyId { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public int PriceTypeId { get; private set; }

    private CounterpartyCategoryPriceType() { }

    private CounterpartyCategoryPriceType(string counterpartyId, int categoryId, int priceTypeId)
    {
        SetCounterpartyId(counterpartyId);
        SetCategoryId(categoryId);
        SetPriceTypeId(priceTypeId);
    }

    public static CounterpartyCategoryPriceType Create(
        string counterpartyId,
        int categoryId,
        int priceTypeId)
        => new(counterpartyId, categoryId, priceTypeId);

    private void SetCounterpartyId(string counterpartyId)
    {
        if (string.IsNullOrWhiteSpace(counterpartyId))
        {
            throw new DomainException(CounterpartyErrors.IdRequired());
        }

        CounterpartyId = counterpartyId.Trim();
    }

    private void SetCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new DomainException(CounterpartyErrors.CategoryInvalid(categoryId));
        }

        CategoryId = categoryId;
    }

    private void SetPriceTypeId(int priceTypeId)
    {
        if (priceTypeId <= 0)
        {
            throw new DomainException(CounterpartyErrors.PriceTypeInvalid(priceTypeId));
        }

        PriceTypeId = priceTypeId;
    }
}
