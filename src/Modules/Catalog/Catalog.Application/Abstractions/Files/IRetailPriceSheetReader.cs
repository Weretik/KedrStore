namespace Catalog.Application.Abstractions.Files;

/// <summary>Reads the 1C price list the owner exported to Excel (spec catalog/010 RP-02).</summary>
public interface IRetailPriceSheetReader
{
    /// <summary>
    /// The rows under the header row that holds «Код» and <paramref name="priceColumnName"/>, or an invalid result
    /// with a Ukrainian message for the <c>file</c> field when the file is not an Excel workbook or lacks the columns.
    /// </summary>
    Result<RetailPriceSheet> Read(Stream content, string priceColumnName);
}
