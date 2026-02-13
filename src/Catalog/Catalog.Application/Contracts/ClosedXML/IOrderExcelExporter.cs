using Catalog.Application.Features.Orders.Create.DTOs;

namespace Catalog.Application.Contracts.ClosedXML;

public interface IOrderExcelExporter
{
    ExcelFile Build(CreateOrderRequest request, Guid orderId);
}
