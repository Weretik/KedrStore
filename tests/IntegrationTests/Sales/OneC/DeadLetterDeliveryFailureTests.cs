using ClosedXML.Excel;
using Sales.Application.Features.Orders.DeliveryFailure;
using Sales.Infrastructure.Exports;
using Sales.Infrastructure.Notifications;

namespace IntegrationTests.Sales.OneC;

public sealed class DeadLetterDeliveryFailureTests
{
    [Fact]
    public void Export_contains_all_lines_and_order_total()
    {
        var notification = CreateNotification();
        var file = new OrderDeliveryFailureExporter().Build(notification);

        using var workbook = new XLWorkbook(new MemoryStream(file.Bytes));
        var sheet = workbook.Worksheet("DeadLetter");

        Assert.Equal("p-1", sheet.Cell("A7").GetString());
        Assert.Equal("Door <A>", sheet.Cell("B7").GetString());
        Assert.Equal(2, sheet.Cell("C7").GetValue<int>());
        Assert.Equal(125m, sheet.Cell("D7").GetValue<decimal>());
        Assert.Equal("p-2", sheet.Cell("A8").GetString());
        Assert.Equal("SUM(D7:D8)", sheet.Cell("D9").FormulaA1);
    }

    [Fact]
    public void Caption_escapes_operator_visible_values()
    {
        var caption = DeadLetterTelegramCaption.Build(CreateNotification());

        Assert.Contains("Помилка доставки до 1С", caption);
        Assert.Contains("&lt;A&gt;", caption);
        Assert.DoesNotContain("<A>", caption);
    }

    private static DeadLetterNotification CreateNotification()
        => new(
            42,
            "SO-20260902-0042",
            "cp-1",
            "Customer <A>",
            "SOAP <failure>",
            10,
            DateTimeOffset.Parse("2026-09-02T10:00:00+00:00"),
            [
                new DeadLetterOrderLine("p-1", "Door <A>", 2, 125m),
                new DeadLetterOrderLine("p-2", "Handle", 1, 75m)
            ]);
}
