namespace Catalog.Application.Features.Orders.Create.DTOs;

public sealed record ExcelFile(string FileName, string ContentType, byte[] Bytes);
