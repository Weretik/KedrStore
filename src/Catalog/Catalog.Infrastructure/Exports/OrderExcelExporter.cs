using Catalog.Application.Contracts.ClosedXML;
using Catalog.Application.Features.Orders.Create.DTOs;
using ClosedXML.Excel;

namespace Catalog.Infrastructure.Exports;

public sealed class OrderExcelExporter : IOrderExcelExporter
{
    public ExcelFile Build(CreateOrderRequest request, Guid orderId)
    {
        using var xlWorkbook = new XLWorkbook();
        var xlWorksheet = xlWorkbook.Worksheets.Add("Order");

        xlWorksheet.Cell("B1").Value = "Заявка:";
        xlWorksheet.Cell("C1").Value = orderId.ToString();

        xlWorksheet.Cell("B2").Value = "Ім’я:";
        xlWorksheet.Cell("C2").Value = request.FirstName;

        xlWorksheet.Cell("B3").Value = "Телефон:";
        xlWorksheet.Cell("C3").Value = request.Phone;

        xlWorksheet.Range("B1:B3").Style.Font.Bold = true;

        xlWorksheet.Cell("A6").Value = "№";
        xlWorksheet.Cell("B6").Value = "ID товару";
        xlWorksheet.Cell("C6").Value = "Назва";
        xlWorksheet.Cell("D6").Value = "К-сть";
        xlWorksheet.Cell("E6").Value = "Ціна";
        xlWorksheet.Cell("F6").Value = "Сума";

        var header = xlWorksheet.Range("A6:F6");
        header.Style.Font.Bold = true;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var row = 7;
        for (var i = 0; i < request.Lines.Count; i++)
        {
            var line = request.Lines[i];

            xlWorksheet.Cell(row, 1).Value = i + 1;
            xlWorksheet.Cell(row, 2).Value = line.ProductId;
            xlWorksheet.Cell(row, 3).Value = line.Title;
            xlWorksheet.Cell(row, 4).Value = line.Quantity;
            xlWorksheet.Cell(row, 5).Value = line.UnitPrice;

            xlWorksheet.Cell(row, 6).FormulaA1 = $"=D{row}*E{row}";

            row++;
        }

        xlWorksheet.Column(4).Style.NumberFormat.Format = "0";
        xlWorksheet.Column(5).Style.NumberFormat.Format = "#,##0.00";
        xlWorksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";

        var tableRange = xlWorksheet.Range(7, 1, row - 1, 6);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        xlWorksheet.Column(3).Style.Alignment.WrapText = true;


        xlWorksheet.Cell(row + 1, 5).Value = "Разом:";
        xlWorksheet.Cell(row + 1, 5).Style.Font.Bold = true;
        xlWorksheet.Cell(row + 1, 6).FormulaA1 = $"=SUM(F7:F{row - 1})";
        xlWorksheet.Cell(row + 1, 6).Style.Font.Bold = true;
        xlWorksheet.Cell(row + 1, 6).Style.NumberFormat.Format = "#,##0.00";

        xlWorksheet.Column(1).Width = 5;   // №
        xlWorksheet.Column(2).Width = 15;  // ID
        xlWorksheet.Column(3).Width = 48;  // Назва
        xlWorksheet.Column(4).Width = 8;   // К-сть
        xlWorksheet.Column(5).Width = 12;  // Ціна
        xlWorksheet.Column(6).Width = 14;  // Сума

        xlWorksheet.SheetView.FreezeRows(6);

        xlWorksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        xlWorksheet.PageSetup.FitToPages(1, 1);
        xlWorksheet.PageSetup.Margins.Top = 0.3;
        xlWorksheet.PageSetup.Margins.Bottom = 0.3;
        xlWorksheet.PageSetup.Margins.Left = 0.3;
        xlWorksheet.PageSetup.Margins.Right = 0.3;

        using var memoryStream = new MemoryStream();
        xlWorkbook.SaveAs(memoryStream);

        var fileName = $"order_{orderId:N}.xlsx";
        return new ExcelFile(
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            memoryStream.ToArray()
        );
    }
}
