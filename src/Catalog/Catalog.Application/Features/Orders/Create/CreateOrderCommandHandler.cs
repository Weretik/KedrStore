using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Features.Orders.Create.DTOs;
using Catalog.Application.Features.Orders.Create.Notifications;

namespace Catalog.Application.Features.Orders.Create;

public sealed class CreateOrderCommandHandler(ITelegramNotifier telegram, IOrderExcelExporter excelExporter)
    : ICommandHandler<CreateOrderCommand, Result<QuickOrderResponse>>
{
    public async ValueTask<Result<QuickOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var orderId = Guid.NewGuid();

        var excel = excelExporter.Build(request, orderId);

        var nowUtc = DateTime.UtcNow;
        var kyivTime = TimeZoneInfo.ConvertTimeFromUtc(
            nowUtc,
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv")
        );
        var parts = new List<string>
        {
            "<b>Нове замовлення</b>",
            $"🆔 <code>{orderId}</code>",
            $"👨‍💼 {request.FirstName}",
            $"📲 <code>{request.Phone}</code>",
            $"📅 {kyivTime:dd.MM.yyyy HH:mm}"
        };

        var captionText = string.Join(Environment.NewLine, parts);


        await telegram.SendDocumentAsync(
            fileName: excel.FileName,
            contentType: excel.ContentType,
            bytes: excel.Bytes,
            caption: captionText,
            cancellationToken: cancellationToken);

        return Result.Success(new QuickOrderResponse(orderId));
    }
}

