using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Notifications;

namespace Catalog.Application.Features.Orders.Create;

public sealed class CreateOrderCommandHandler(ITelegramNotifier telegram, IOrderExcelExporter excelExporter)
    : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var orderId = Guid.NewGuid();

        var excel = excelExporter.Build(request, orderId);

        await telegram.SendDocumentAsync(
            fileName: excel.FileName,
            contentType: excel.ContentType,
            bytes: excel.Bytes,
            caption: $"<b>Нове замовлення</b>\nID: <code>{orderId}</code>",
            cancellationToken: cancellationToken);

        return Result.Success(orderId);
    }
}

