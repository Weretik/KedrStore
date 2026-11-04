using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Features.Orders.Create;
using Catalog.Application.Features.Orders.Create.DTOs;
using Catalog.Application.Features.Orders.Create.Notifications;

namespace UnitTests.Catalog.Application;

public sealed class CreateQuickOrderHandlerTests
{
    [Fact]
    public async Task Handle_EscapesCustomerTextInTheTelegramHtmlCaption()
    {
        var telegram = new RecordingTelegram();
        var handler = new CreateOrderCommandHandler(telegram, new StubExporter());
        var request = new CreateOrderRequest(
            "<b>Іван & Co</b>",
            "+38 (067) 000-00-00",
            [new CreateOrderLineRequest("1", "Ручка", 100m, 1)]);

        var result = await handler.Handle(new CreateOrderCommand(request), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("&lt;b&gt;Іван &amp; Co&lt;/b&gt;", telegram.Caption);
        Assert.DoesNotContain("<b>Іван", telegram.Caption);
        Assert.Matches("🆔 <b>[0-9A-F]{8}</b> <code>", telegram.Caption);
    }

    private sealed class RecordingTelegram : ITelegramNotifier
    {
        public string Caption { get; private set; } = string.Empty;

        public Task SendMessageAsync(string text, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SendDocumentAsync(
            string fileName,
            string contentType,
            byte[] bytes,
            string? caption = null,
            CancellationToken cancellationToken = default)
        {
            Caption = caption ?? string.Empty;
            return Task.CompletedTask;
        }
    }

    private sealed class StubExporter : IOrderExcelExporter
    {
        public ExcelFile Build(CreateOrderRequest request, Guid orderId) =>
            new("order.xlsx", "application/octet-stream", []);
    }
}
