using ClosedXML.Excel;
using Sales.Application.Features.Orders.DeliveryFailure;

namespace Sales.Infrastructure.Exports;

public sealed class OrderDeliveryFailureExporter : IOrderDeliveryFailureExporter
{
    public DeadLetterExcelFile Build(DeadLetterNotification notification)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DeadLetter");

        worksheet.Cell("A1").Value = "Статус";
        worksheet.Cell("B1").Value = "DeadLetter";
        worksheet.Cell("A2").Value = "Замовлення";
        worksheet.Cell("B2").Value = notification.OrderNumber;
        worksheet.Cell("A3").Value = "Контрагент";
        worksheet.Cell("B3").Value = notification.CounterpartyName;
        worksheet.Cell("C3").Value = notification.CounterpartyId;
        worksheet.Cell("A4").Value = "Помилка";
        worksheet.Cell("B4").Value = notification.ErrorMessage ?? string.Empty;

        worksheet.Range("A1:A4").Style.Font.Bold = true;

        var headerRow = 6;
        var headers = new[] { "ID товару", "Назва", "Кількість", "Сума" };
        for (var column = 0; column < headers.Length; column++)
            worksheet.Cell(headerRow, column + 1).Value = headers[column];

        worksheet.Range(headerRow, 1, headerRow, headers.Length).Style.Font.Bold = true;
        worksheet.Range(headerRow, 1, headerRow, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = headerRow + 1;
        foreach (var line in notification.Lines)
        {
            worksheet.Cell(row, 1).Value = line.ProductId;
            worksheet.Cell(row, 2).Value = line.ProductName;
            worksheet.Cell(row, 3).Value = line.Quantity;
            worksheet.Cell(row, 4).Value = line.Amount;
            row++;
        }

        worksheet.Cell(row, 3).Value = "Разом:";
        worksheet.Cell(row, 3).Style.Font.Bold = true;
        worksheet.Cell(row, 4).FormulaA1 = $"=SUM(D{headerRow + 1}:D{row - 1})";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Column(3).Style.NumberFormat.Format = "0";
        worksheet.Column(4).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Columns().AdjustToContents();
        worksheet.Column(2).Width = Math.Min(60, Math.Max(20, worksheet.Column(2).Width));

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new DeadLetterExcelFile(
            $"dead-letter-order-{notification.OrderId}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            stream.ToArray());
    }
}
