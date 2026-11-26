using Sales.Application.Features.QuickOrders.ToOneC;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders.ToOneC;

/// <summary>Spec sales/009, R-010: the 1C order comment names the site order, the buyer, and the Kyiv time.</summary>
public sealed class QuickOrderOneCCommentTests
{
    [Fact]
    public void Build_GivesTheNumber_TheBuyer_AndTheKyivTime()
    {
        var order = QuickOrder.Create(
            QuickOrderId.Create(Guid.Parse("52992220-6a4e-4d8b-9e21-3c5a7b9d1f04")), " Іван ", "+380670000000",
            [QuickOrderItem.Create("101", "Ручка", 10m, 1).Value],
            new DateTimeOffset(2026, 11, 26, 10, 15, 0, TimeSpan.Zero)).Value;

        Assert.Equal("Сайт № 52992220, Іван, +380670000000, 26.11.2026 12:15", QuickOrderOneCComment.Build(order));
    }
}
