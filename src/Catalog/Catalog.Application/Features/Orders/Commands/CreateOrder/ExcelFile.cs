namespace Catalog.Application.Features.Orders.Commands.CreateOrder;

public sealed record ExcelFile(string FileName, string ContentType, byte[] Bytes);
