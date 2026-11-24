using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

namespace UnitTests.Sales.QuickOrders;

public sealed class QuickOrderNumberSearchTests
{
    private static readonly Guid OrderId = Guid.Parse("52992220-9449-43ca-aace-c459869adc3b");

    [Fact]
    public void NumberOf_IsTheFirstEightHexDigits_AsOnTheSiteAndInTelegram() =>
        Assert.Equal("52992220", QuickOrderNumberSearch.NumberOf(OrderId));

    [Theory]
    [InlineData("52992220")]
    [InlineData("№52992220")]
    [InlineData("#5299222a")]
    public void RangeOf_ANumber_CoversEveryIdWithThatStart(string word)
    {
        var range = QuickOrderNumberSearch.RangeOf(word);

        Assert.NotNull(range);
        var prefix = word.TrimStart('#', '№').ToLowerInvariant();
        Assert.Equal(Guid.Parse($"{prefix}-0000-0000-0000-000000000000"), range.Value.From);
        Assert.Equal(Guid.Parse($"{prefix}-ffff-ffff-ffff-ffffffffffff"), range.Value.To);
    }

    [Fact]
    public void RangeOf_AWholeId_IsThatId() =>
        Assert.Equal((OrderId, OrderId), QuickOrderNumberSearch.RangeOf(OrderId.ToString()));

    [Theory]
    [InlineData("Ірина")]
    [InlineData("4315238")]
    [InlineData("529922201")]
    [InlineData("5299222g")]
    public void RangeOf_OtherWords_IsNull(string word) => Assert.Null(QuickOrderNumberSearch.RangeOf(word));
}
