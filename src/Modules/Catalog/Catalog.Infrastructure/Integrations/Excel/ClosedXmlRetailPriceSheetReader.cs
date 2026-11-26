using Ardalis.Result;
using Catalog.Application.Abstractions.Files;
using ClosedXML.Excel;

namespace Catalog.Infrastructure.Integrations.Excel;

/// <summary>
/// Reads the first worksheet of the 1C price list exported to Excel (spec catalog/010 RP-02): finds the header row with
/// «Код» and the retail price column by name (<see cref="RetailPriceSheetHeader"/>), then reads every row below it.
/// </summary>
public sealed class ClosedXmlRetailPriceSheetReader : IRetailPriceSheetReader
{
    public Result<RetailPriceSheet> Read(Stream content, string priceColumnName)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(content);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // ClosedXML has no single exception type for a file that is not a workbook (zip, XML, or format errors).
            return Invalid("Це не файл Excel (.xlsx). Збережіть прайс з 1С у форматі .xlsx.");
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault();
            var header = sheet is null ? null : RetailPriceSheetHeader.Find(sheet, priceColumnName);
            if (sheet is null || header is null)
                return Invalid(
                    $"У файлі немає рядка з колонками «{RetailPriceSheetHeader.CodeColumnName}» і «{priceColumnName.Trim()}».");

            var (headerRow, codeColumn, priceColumn) = header;
            var rows = new List<RetailPriceSheetRow>();
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var row = headerRow + 1; row <= lastRow; row++)
            {
                var code = sheet.Cell(row, codeColumn).GetFormattedString().Trim();
                if (code.Length > 0)
                    rows.Add(new RetailPriceSheetRow(code, Number(sheet.Cell(row, priceColumn))));
            }

            return Result.Success(new RetailPriceSheet(rows));
        }
    }

    /// <summary>A number cell, or text such as «341,5» or «341.5»; anything else is no price.</summary>
    private static decimal? Number(IXLCell cell)
    {
        if (cell.DataType == XLDataType.Number)
            return (decimal)cell.GetDouble();

        var text = cell.GetFormattedString().Trim().Replace(" ", string.Empty).Replace(',', '.');
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static Result<RetailPriceSheet> Invalid(string message)
        => Result<RetailPriceSheet>.Invalid(new ValidationError("file", message, null, ValidationSeverity.Error));
}
