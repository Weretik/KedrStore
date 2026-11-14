using ClosedXML.Excel;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>The Excel file of a quick order: customer, lines with sums, and the total.</summary>
public static class QuickOrderExcelFile
{
    public static string NameOf(QuickOrderPlaced order) => $"order_{order.OrderId:N}.xlsx";

    public static byte[] Build(QuickOrderPlaced order)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Order");

        sheet.Cell("B1").Value = "Заявка:";
        sheet.Cell("C1").Value = order.OrderId.ToString();

        sheet.Cell("B2").Value = "Ім’я:";
        sheet.Cell("C2").Value = order.FirstName;

        sheet.Cell("B3").Value = "Телефон:";
        sheet.Cell("C3").Value = order.Phone;

        sheet.Range("B1:B3").Style.Font.Bold = true;

        sheet.Cell("A6").Value = "№";
        sheet.Cell("B6").Value = "ID товару";
        sheet.Cell("C6").Value = "Назва";
        sheet.Cell("D6").Value = "К-сть";
        sheet.Cell("E6").Value = "Ціна";
        sheet.Cell("F6").Value = "Сума";

        var header = sheet.Range("A6:F6");
        header.Style.Font.Bold = true;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var row = 7;
        for (var i = 0; i < order.Lines.Count; i++)
        {
            var line = order.Lines[i];

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = line.ProductId;
            sheet.Cell(row, 3).Value = line.Title;
            sheet.Cell(row, 4).Value = line.Quantity;
            sheet.Cell(row, 5).Value = line.UnitPrice;
            sheet.Cell(row, 6).FormulaA1 = $"=D{row}*E{row}";

            row++;
        }

        sheet.Column(4).Style.NumberFormat.Format = "0";
        sheet.Column(5).Style.NumberFormat.Format = "#,##0.00";
        sheet.Column(6).Style.NumberFormat.Format = "#,##0.00";

        if (order.Lines.Count > 0)
        {
            var table = sheet.Range(7, 1, row - 1, 6);
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Cell(row + 1, 5).Value = "Разом:";
            sheet.Cell(row + 1, 5).Style.Font.Bold = true;
            sheet.Cell(row + 1, 6).FormulaA1 = $"=SUM(F7:F{row - 1})";
            sheet.Cell(row + 1, 6).Style.Font.Bold = true;
            sheet.Cell(row + 1, 6).Style.NumberFormat.Format = "#,##0.00";
        }

        sheet.Column(3).Style.Alignment.WrapText = true;

        sheet.Column(1).Width = 5;   // №
        sheet.Column(2).Width = 15;  // ID
        sheet.Column(3).Width = 48;  // Назва
        sheet.Column(4).Width = 8;   // К-сть
        sheet.Column(5).Width = 12;  // Ціна
        sheet.Column(6).Width = 14;  // Сума

        sheet.SheetView.FreezeRows(6);

        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 1);
        sheet.PageSetup.Margins.Top = 0.3;
        sheet.PageSetup.Margins.Bottom = 0.3;
        sheet.PageSetup.Margins.Left = 0.3;
        sheet.PageSetup.Margins.Right = 0.3;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
