using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Application.Catalog.Shared;
using Application.Catalog.ImportCatalogFromXml;
using Infrastructure.Common.Contracts;

namespace Infrastructure.Catalog.Import;

/// <summary>
/// Robust XML parser for KEDR catalog.
/// - Fixes only encoding="utf8"
/// - Uses ONE MemoryStream (no disposed errors)
/// - Streams XML
/// - Skips ANY broken <product>
/// - Never throws XmlException
/// - Does not modify XML structure
/// </summary>
public class CatalogXmlParser(ILogger<CatalogXmlParser> logger) : ICatalogXmlParser
{
    public async Task<CatalogParseResult> ParseAsync(
        Stream xml,
        int productTypeId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("XML streaming parse started");

        // ----------------------------------------------------------------------
        // COPY INPUT STREAM → MEMORYSTREAM (SAFE, NO DISPOSE, NO DOUBLE READING)
        // ----------------------------------------------------------------------
        xml.Position = 0;
        var ms = new MemoryStream();
        await xml.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        // Read text
        string xmlText;
        using (var sr = new StreamReader(ms, Encoding.UTF8, leaveOpen: true))
        {
            xmlText = await sr.ReadToEndAsync(cancellationToken);
        }

        // Fix
        xmlText = xmlText.Replace("encoding=\"utf8\"", "encoding=\"utf-8\"", StringComparison.OrdinalIgnoreCase);
        xmlText = xmlText.Replace("<Свойства не назначены>", "");

        // Write back to same memory stream
        var bytes = Encoding.UTF8.GetBytes(xmlText);
        ms = new MemoryStream(bytes);
        ms.Position = 0;

        // Use ms for further parsing
        xml = ms;

        // ----------------------------------------------------------------------
        // XML Reader Settings
        // ----------------------------------------------------------------------

        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreWhitespace = true,
            IgnoreComments = true,
            XmlResolver = null
        };

        var categories = GetBaseCategories(productTypeId);
        var products = new List<ProductDto>();

        using var reader = XmlReader.Create(xml, settings);

        // ----------------------------------------------------------------------
        // STREAM THROUGH XML
        // ----------------------------------------------------------------------
        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element && reader.Name == "product")
            {
                XElement? element = null;

                // HARD SKIP INVALID PRODUCT BLOCK
                try
                {
                    element = await XNode.ReadFromAsync(reader, cancellationToken) as XElement;
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Пропуск product: поврежденный XML ({Message})", ex.Message);
                    continue;
                }

                if (element == null)
                {
                    logger.LogWarning("Пропуск product: пустой элемент");
                    continue;
                }

                // TRY PARSE PRODUCT
                if (!TryParseProduct(element, productTypeId, categories, out var dto))
                    continue;

                products.Add(dto);
            }
        }

        logger.LogInformation("XML streaming parse completed");
        return new CatalogParseResult(categories, products);
    }

    // ----------------------------------------------------------------------
    // PARSE PRODUCT
    // ----------------------------------------------------------------------

    private bool TryParseProduct(
        XElement xmlElement,
        int productTypeId,
        List<ImportCategoryDto> categories,
        out ProductDto product)
    {
        product = default!;

        try
        {
            var id = TryGetInt(xmlElement.Element("id")?.Value);
            var name = (TryGetValue(xmlElement, "name") ?? "").Trim();
            var idR = TryGetInt(xmlElement.Element("idR")?.Value);
            var nameR = (TryGetValue(xmlElement, "nameR") ?? "").Trim();
            var count = TryGetInt(xmlElement.Element("count")?.Value);

            var skipWords = new[] { "KEDR", "Стенди", "Замiна KEDR, CLASS" };

            if (id <= 0 ||
                idR <= 0 ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(nameR) ||
                count < 0 ||
                skipWords.Any(w =>
                    nameR.Equals(w, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (productTypeId == 2)
                nameR = NormalizeCategoryName(nameR);

            var parentMap = GetParentMap(productTypeId);
            parentMap.TryGetValue(idR, out var parentBaseId);

            if (productTypeId == 1 && parentBaseId == 0)
                parentBaseId = 103;

            EnsureCategoryById(categories, idR, nameR, parentBaseId);

            var pricesNode = xmlElement.Element("prices");
            if (pricesNode == null || !pricesNode.HasElements)
                return false;

            var prices = ReadPrices(pricesNode);

            var photo =
                $"https://raw.githubusercontent.com/Kedr-Class/images/refs/heads/main/furniture/{id}.jpg";

            product = new ProductDto(
                Id: id,
                Name: name,
                CategoryId: idR,
                ProductTypeId: productTypeId,
                Photo: photo,
                Stock: count,
                Prices: prices);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Пропуск product id={Id}: ошибка парсинга ({Message})",
                xmlElement.Element("id")?.Value, ex.Message);

            return false;
        }
    }

    // ----------------------------------------------------------------------
    // HELPERS
    // ----------------------------------------------------------------------

    static XElement? Child(XElement parent, string localName) =>
        parent.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));

    static string? TryGetValue(XElement parent, string localName) =>
        Child(parent, localName)?.Value;

    static int TryGetInt(string? v) =>
        int.TryParse(v, out var x) ? x : 0;

    static decimal TryGetDecimal(string? v) =>
        decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
            ? x
            : 0m;

    static List<ProductPriceDto> ReadPrices(XElement pricesNode)
    {
        var list = new List<ProductPriceDto>();

        foreach (var price in pricesNode.Elements())
        {
            var name = price.Name.LocalName;
            if (!name.StartsWith("price_", StringComparison.OrdinalIgnoreCase))
                continue;

            list.Add(new ProductPriceDto(
                PriceType: name,
                Amount: TryGetDecimal(price.Value),
                CurrencyIso: "UAH"));
        }

        return list;
    }

    static string NormalizeCategoryName(string name)
    {
        const string prefix = "Kedr ";
        name = name.Trim();

        return name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? name[prefix.Length..].TrimStart()
            : name;
    }

    static Dictionary<int, int> GetParentMap(int productTypeId) =>
        productTypeId switch
        {
            1 => new()
            {
                [19424] = 101, [8349] = 101, [8344] = 101, [26971] = 101,
                [7259] = 102, [7258] = 102, [27083] = 102, [19425] = 102, [8209] = 102
            },

            2 => new()
            {
                [4457] = 104, [6139] = 104, [1707] = 104,
                [5851] = 105, [1304] = 105, [2722] = 105, [2775] = 105, [2716] = 105,
                [5999] = 106, [5853] = 106, [26949] = 106, [6982] = 106,
                [5854] = 106, [5904] = 106, [5915] = 106, [6488] = 106,
                [6560] = 107, [2680] = 107, [26930] = 107, [26929] = 107,
                [5852] = 107, [4555] = 107, [27124] = 107,
                [5513] = 108, [5273] = 108, [2197] = 108, [6108] = 108, [2321] = 108
            },

            _ => new()
        };

    static List<ImportCategoryDto> GetBaseCategories(int productTypeId) =>
        productTypeId switch
        {
            1 => new()
            {
                new ImportCategoryDto(101, "Міжкімнатні двері", "n101"),
                new ImportCategoryDto(102, "Вхідні двері", "n102"),
                new ImportCategoryDto(103, "Інше", "n103")
            },

            2 => new()
            {
                new ImportCategoryDto(104, "Завіси", "n104"),
                new ImportCategoryDto(105, "Замки", "n105"),
                new ImportCategoryDto(106, "Ручки", "n106"),
                new ImportCategoryDto(107, "Циліндри", "n107"),
                new ImportCategoryDto(108, "Міжкімнатні механізми", "n108")
            },

            _ => throw new ArgumentOutOfRangeException(nameof(productTypeId))
        };

    static void EnsureCategoryById(List<ImportCategoryDto> list, int id, string name, int parentBaseId)
    {
        if (list.Any(x => x.Id == id))
            return;

        var parentPath = parentBaseId == 0
            ? ""
            : list.FirstOrDefault(x => x.Id == parentBaseId)?.Path ?? "";

        var path = string.IsNullOrWhiteSpace(parentPath) ? $"n{id}" : $"{parentPath}.n{id}";

        list.Add(new ImportCategoryDto(id, name, path));
    }
}
