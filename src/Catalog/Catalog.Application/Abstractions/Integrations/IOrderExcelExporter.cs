using Catalog.Application.Features.Orders.Commands.CreateOrder;

namespace Catalog.Application.Abstractions.Integrations;

public interface IOrderExcelExporter
{
    ExcelFile Build(CreateOrderRequest request, Guid orderId);
}
