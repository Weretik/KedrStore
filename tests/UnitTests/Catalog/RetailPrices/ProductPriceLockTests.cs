using BuildingBlocks.Domain.Identifiers;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products.Prices;
using NodaMoney;

namespace UnitTests.Catalog.RetailPrices;

/// <summary>Spec catalog/010 SC-RP01: a retail price from the owner's file and its lock from the 1C sync.</summary>
public sealed class ProductPriceLockTests
{
    private const string Root = "000005513";
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateFromFile_StoresUahRoundedAndLocksWhenAsked()
    {
        var price = ProductPrice.CreateFromFile(Root, ProductId.Create(561), PriceTypeId.Create(11), 195.755m,
            lockFromOneC: true, Now).Value;

        Assert.Equal(195.76m, price.Amount);
        Assert.Equal("UAH", price.CurrencyIso);
        Assert.Equal(Root, price.ProductTypeIdOneC);
        Assert.True(price.IsLockedFromOneC);
        Assert.Equal(Now, price.LockedFromOneCAt);
        Assert.False(price.CanBeRemovedByOneC);
    }

    [Fact]
    public void CreateFromFile_WithoutLock_LeavesThePriceToOneC()
    {
        var price = ProductPrice.CreateFromFile(Root, ProductId.Create(561), PriceTypeId.Create(11), 201m,
            lockFromOneC: false, Now).Value;

        Assert.False(price.IsLockedFromOneC);
        Assert.Null(price.LockedFromOneCAt);
        Assert.True(price.CanBeRemovedByOneC);
    }

    [Fact]
    public void SetFromFile_ReportsWhetherTheAmountChanged_AndReplacesTheLockState()
    {
        var price = OneCPrice(150m, "USD");

        var changed = price.SetFromFile(201m, lockFromOneC: true, Now);
        var same = price.SetFromFile(201m, lockFromOneC: false, Now.AddHours(1));

        Assert.True(changed.Value);
        Assert.False(same.Value);
        Assert.Equal(201m, price.Amount);
        Assert.Equal("UAH", price.CurrencyIso);
        Assert.False(price.IsLockedFromOneC);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5_000_001)]
    public void SetFromFile_RefusesAnAmountThePriceRulesRefuse(decimal amount)
    {
        var price = OneCPrice(150m);

        var result = price.SetFromFile(amount, lockFromOneC: true, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(150m, price.Amount);
        Assert.False(price.IsLockedFromOneC);
    }

    [Fact]
    public void UpdateFromOneC_RefusesALockedPrice_AndKeepsTheFileAmount()
    {
        var price = OneCPrice(150m);
        price.SetFromFile(201m, lockFromOneC: true, Now);

        var result = price.UpdateFromOneC(new Money(140m, "UAH"));

        Assert.Equal("Catalog.ProductPrice.LockedFromOneC", result.Error!.Code);
        Assert.Equal(201m, price.Amount);
    }

    [Fact]
    public void UnlockFromOneC_LetsTheNextSyncChangeThePrice()
    {
        var price = OneCPrice(150m);
        price.SetFromFile(201m, lockFromOneC: true, Now);

        price.UnlockFromOneC();
        var result = price.UpdateFromOneC(new Money(140m, "UAH"));

        Assert.True(result.IsSuccess);
        Assert.Equal(140m, price.Amount);
        Assert.True(price.CanBeRemovedByOneC);
    }

    private static ProductPrice OneCPrice(decimal amount, string currency = "UAH")
        => ProductPrice.Create(Root, ProductId.Create(561), PriceTypeId.Create(11), new Money(amount, currency)).Value;
}
