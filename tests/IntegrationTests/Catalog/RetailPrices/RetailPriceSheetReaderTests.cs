using Ardalis.Result;
using Catalog.Application.Abstractions.Files;
using Catalog.Infrastructure.Integrations.Excel;
using ClosedXML.Excel;

namespace IntegrationTests.Catalog.RetailPrices;

/// <summary>Spec catalog/010 SC-RP02: the 1C price list exported to Excel, as the owner uploads it.</summary>
public sealed class RetailPriceSheetReaderTests
{
    private const string Retail = "Розница халтмаг 30%";
    private readonly ClosedXmlRetailPriceSheetReader _reader = new();

    [Fact]
    public void Read_FindsTheColumnsAfterTitleRows_AndReadsCodesAsTextOrNumbers()
    {
        using var file = Workbook(sheet =>
        {
            sheet.Cell("A1").Value = "Каталог товаров";
            sheet.Cell("A2").Value = "  26.11.26";
            sheet.Cell("A5").Value = "№";
            sheet.Cell("B5").Value = "Код";
            sheet.Cell("C5").Value = "Наименование товаров";
            sheet.Cell("D5").Value = "**2 СУПЕР ОПТ";
            sheet.Cell("E5").Value = " розница ХАЛТМАГ 30% ";
            sheet.Cell("C6").Value = "KEDR";
            sheet.Cell("A7").Value = "1";
            sheet.Cell("B7").Value = "561";
            sheet.Cell("D7").Value = 2.65;
            sheet.Cell("E7").Value = 201;
            sheet.Cell("B8").Value = 8202;
            sheet.Cell("E8").Value = "341,5";
            sheet.Cell("B9").Value = "3066";
        });

        var result = _reader.Read(file, Retail);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [new RetailPriceSheetRow("561", 201m), new RetailPriceSheetRow("8202", 341.5m),
                new RetailPriceSheetRow("3066", null)],
            result.Value.Rows);
    }

    [Fact]
    public void Read_ReportsAMissingPriceColumn_InUkrainian()
    {
        using var file = Workbook(sheet =>
        {
            sheet.Cell("A1").Value = "Код";
            sheet.Cell("B1").Value = "*ОПТ ГРН";
        });

        var result = _reader.Read(file, Retail);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal("У файлі немає рядка з колонками «Код» і «Розница халтмаг 30%».",
            Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    [Fact]
    public void Read_ReportsAFileThatIsNotExcel()
    {
        using var file = new MemoryStream("not a workbook"u8.ToArray());

        var result = _reader.Read(file, Retail);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal("Це не файл Excel (.xlsx). Збережіть прайс з 1С у форматі .xlsx.",
            Assert.Single(result.ValidationErrors).ErrorMessage);
    }

    private static MemoryStream Workbook(Action<IXLWorksheet> fill)
    {
        using var workbook = new XLWorkbook();
        fill(workbook.AddWorksheet("Sheet1"));
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
