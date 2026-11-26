using ClosedXML.Excel;

namespace Catalog.Infrastructure.Integrations.Excel;

/// <summary>
/// The header row of the 1C price list: the first row with a «Код» cell and a cell named like the retail price type,
/// compared trimmed and case-insensitively; title rows above it and the column order do not matter.
/// </summary>
internal sealed record RetailPriceSheetHeader(int Row, int CodeColumn, int PriceColumn)
{
    public const string CodeColumnName = "Код";

    public static RetailPriceSheetHeader? Find(IXLWorksheet sheet, string priceColumnName)
    {
        foreach (var row in sheet.RowsUsed())
        {
            int? code = null, price = null;
            foreach (var cell in row.CellsUsed())
            {
                var text = cell.GetFormattedString().Trim();
                if (Same(text, CodeColumnName))
                    code ??= cell.Address.ColumnNumber;
                else if (Same(text, priceColumnName))
                    price ??= cell.Address.ColumnNumber;
            }

            if (code is not null && price is not null)
                return new RetailPriceSheetHeader(row.RowNumber(), code.Value, price.Value);
        }

        return null;
    }

    private static bool Same(string text, string name)
        => string.Equals(text, name.Trim(), StringComparison.OrdinalIgnoreCase);
}
