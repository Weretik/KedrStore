using Sales.Application.Features.QuickOrders.Messages;
using Sales.Infrastructure.QuickOrders;

namespace UnitTests.Sales.QuickOrders;

public sealed class QuickOrderTelegramCaptionTests
{
    private static readonly QuickOrderPlaced Order = new(
        Guid.Parse("019a8b1c-2d3e-7f40-8a5b-6c7d8e9f0a1b"),
        "<b>Іван & Co</b>",
        "+38 (067) 000-00-00",
        [new QuickOrderLine("1", "Ручка", 100m, 1)],
        new DateTimeOffset(2026, 11, 14, 10, 5, 0, TimeSpan.Zero));

    [Fact]
    public void Caption_EscapesCustomerText()
    {
        var caption = QuickOrderTelegramCaption.Build(Order);

        Assert.Contains("&lt;b&gt;Іван &amp; Co&lt;/b&gt;", caption);
        Assert.DoesNotContain("<b>Іван", caption);
    }

    [Fact]
    public void Caption_ShowsTheOrderId_AndTheKyivTimeOfTheOrder()
    {
        var caption = QuickOrderTelegramCaption.Build(Order);

        Assert.Contains("🆔 <b>019A8B1C</b> <code>019a8b1c-2d3e-7f40-8a5b-6c7d8e9f0a1b</code>", caption);
        Assert.Contains("📅 14.11.2026 12:05", caption);
    }

    [Fact]
    public void ExcelFile_IsNamedByTheOrderId()
    {
        Assert.Equal("order_019a8b1c2d3e7f408a5b6c7d8e9f0a1b.xlsx", QuickOrderExcelFile.NameOf(Order));
        Assert.NotEmpty(QuickOrderExcelFile.Build(Order));
    }
}
