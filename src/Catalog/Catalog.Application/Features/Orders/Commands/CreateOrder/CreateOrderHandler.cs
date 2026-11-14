using System.Net;
using Catalog.Application.Abstractions.Integrations;

namespace Catalog.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(ITelegramNotifier telegram, IOrderExcelExporter excelExporter)
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
            $"🆔 <b>{orderId.ToString("N")[..8].ToUpperInvariant()}</b> <code>{orderId}</code>",
            $"👨‍💼 {WebUtility.HtmlEncode(request.FirstName)}",
            $"📲 <code>{WebUtility.HtmlEncode(request.Phone)}</code>",
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

