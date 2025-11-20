using Application.Catalog.ImportCatalogFromXml;
using Application.Catalog.Shared;
using Infrastructure.Common.Contracts;

namespace Infrastructure.Catalog.Import;

/// <summary>
/// Kedr XML catalog parser.
/// Responsible for reading input XML, fixing problematic encodings,
/// building categories and product lists.
/// </summary>
public class CatalogXmlParser(ILogger<CatalogXmlParser> logger) : ICatalogXmlParser
{
    /// <summary>
    /// Asynchronously parses an XML catalog and returns categories and products.
    /// </summary>
    /// <param name="xml">Input XML file (stream)</param>
    /// <param name="productTypeId">Product type (1 – fittings, 2 – doors)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of catalog parsing</returns>
    public async Task<CatalogParseResult> ParseAsync(Stream xml, int productTypeId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("XML parse started");

        // The supplier writes encoding="utf8" — .NET does not understand this, so we correct it to “utf-8.”
        using (var streamReader = new StreamReader(xml, Encoding.UTF8, leaveOpen: true))
        {
            var text = await streamReader.ReadToEndAsync(cancellationToken);
            text = Regex.Replace(text, @"encoding\s*=\s*[""']utf8[""']", "encoding=\"utf-8\"", RegexOptions.IgnoreCase);
            xml = new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        // Safe XML reading settings — no DTD, no resolver, no comments
        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            CloseInput = false
        };

        var categories = GetBaseCategories(productTypeId);
        var products = new List<ProductDto>();

        using var xmlReader = XmlReader.Create(xml, settings);
        var xmlDocument = await XDocument.LoadAsync(xmlReader, LoadOptions.None, cancellationToken);

        var root = xmlDocument.Root?.Elements("product") ?? [];

        foreach (var xmlElement in root)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = TryGetInt(xmlElement.Element("id")?.Value);
            var name = (TryGetValue(xmlElement, "name") ?? "").Trim();
            var idR = TryGetInt(xmlElement.Element("idR")?.Value);
            var nameR = (TryGetValue(xmlElement, "nameR") ?? "").Trim();
            var count = TryGetInt(xmlElement.Element("count")?.Value);

            var skipWords = new[] { "KEDR", "Стенди", "Замiна KEDR, CLASS" };
            // Skip invalid elements (some providers return empty id/name values)
            if (id <= 0
                || idR <= 0
                || IsNullOrWhite(name)
                || IsNullOrWhite(nameR)
                || skipWords.Any(words => nameR.Equals(words, StringComparison.OrdinalIgnoreCase))
                || count < 0)
            {
                logger.LogWarning("Пропуск product: некоректні поля " +
                                  "(id={Id}, idR={IdR}, name='{Name}', nameR='{NameR}', count='{count}')",
                    id, idR, name, nameR, count);
                continue;
            }

            // For productTypeId=2 (fittings), remove the prefix “Kedr” from category names
            if(productTypeId == 2) nameR = NormalizeCategoryName(nameR);

            var parentMap = GetParentMap(productTypeId);
            parentMap.TryGetValue(idR, out var parentBaseId);

            // If no parent category is found, assign to “Other” (id=103)
            if (productTypeId == 1 && parentBaseId == 0) parentBaseId = 103;

            EnsureCategoryById(categories, idR, nameR, parentBaseId);

            var pricesNode = xmlElement.Element("prices");
            if (pricesNode is null || !pricesNode.HasElements) continue;

            var prices = ReadPrices(pricesNode);

            // Photos are generated according to a fixed template on GitHub
            var photoLink = $"https://raw.githubusercontent.com/Kedr-Class/images/refs/heads/main/furniture/{id}.jpg";

            products.Add(new ProductDto(
                Id: id,
                Name: name,
                CategoryId: idR,
                ProductTypeId: productTypeId,
                Photo: photoLink,
                Stock: count,
                Prices: prices
            ));
        }

        logger.LogInformation("XML parse stopped");
        return await Task.FromResult(new CatalogParseResult(categories, products));
    }

    /// <summary>
    /// Basic categories for each product type (id = fixed).
    /// </summary>
    static List<ImportCategoryDto> GetBaseCategories(int productTypeId)
    {
        return productTypeId switch
        {
            1 => // Doors
            [
                new ImportCategoryDto(101, "Міжкімнатні двері", "n101"),
                new ImportCategoryDto(102, "Вхідні двері", "n102"),
                new ImportCategoryDto(103, "Інше", "n103")
            ],
            2 => // Fittings
            [
                new ImportCategoryDto(104, "Завіси", "n104"),
                new ImportCategoryDto(105, "Замки", "n105"),
                new ImportCategoryDto(106, "Ручки", "n106"),
                new ImportCategoryDto(107, "Циліндри", "n107"),
                new ImportCategoryDto(108, "Міжкімнатні механізми", "n108")

            ],
            _ => throw new ArgumentOutOfRangeException(nameof(productTypeId), productTypeId, null)
        };
    }

    static XElement? Child(XElement parent, string localName) =>
        parent.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase));

    static string? TryGetValue(XElement parent, string localName) => Child(parent, localName)?.Value;

    static int TryGetInt(string? element)
        => int.TryParse(element, out var value) ? value : 0;

    static decimal TryGetDecimal(string? element)
        => decimal.TryParse(element, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    static bool IsNullOrWhite(string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Removes the service prefix “Kedr” from category names.
    /// </summary>
    static string NormalizeCategoryName(string name)
    {
        const string deletedString = "Kedr ";
        var resultName = name.Trim();
        if (resultName.StartsWith(deletedString, StringComparison.OrdinalIgnoreCase))
        {
            resultName = resultName.Substring(deletedString.Length).TrimStart();
        }
        return resultName;
    }

    /// <summary price_="nodes and generates a list of prices.">
    /// Reads all
    /// </summary>
    static List<ProductPriceDto> ReadPrices(XElement pricesNode)
    {
        var result = new List<ProductPriceDto>();
        foreach (var price in pricesNode.Elements())
        {
            var priceType = price.Name.LocalName;
            if (!priceType.StartsWith("price_", StringComparison.OrdinalIgnoreCase)) continue;

            var dto = new ProductPriceDto(
                PriceType: priceType.Trim(),
                Amount: TryGetDecimal(price.Value),
                CurrencyIso: "UAH");

            result.Add(dto);
        }
        return result;
    }

    /// <summary>
    /// Category mapping (idR → base category). Used to build a tree.
    /// </summary>
    static Dictionary<int, int> GetParentMap(int productTypeId) => productTypeId switch
    {
        1 => new()
        {
            [19424] = 101, [8349] = 101, [8344] = 101, [26971] = 101,
            [7259] = 102, [7258] = 102, [27083] = 102, [19425] = 102, [8209] = 102,
        },
        2 => new()
        {
            [4457] = 104, [6139] = 104, [1707] = 104,
            [5851] = 105, [1304] = 105, [2722] = 105, [2775] = 105, [2716] = 105,
            [5999] = 106, [5853] = 106, [26949] = 106, [6982] = 106, [5854] = 106, [5904] = 106, [5915] = 106, [6488] = 106,
            [6560] = 107, [2680] = 107, [26930] = 107, [26929] = 107, [5852] = 107, [4555] = 107, [27124] = 107,
            [5513] = 108, [5273] = 108, [2197] = 108, [6108] = 108, [2321] = 108
        },
        _ => new()
    };

    /// <summary>
    /// If the category does not yet exist, adds it with the correct path.
    /// </summary>
    static void EnsureCategoryById(List<ImportCategoryDto> list, int id, string name, int parentBaseId)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Id == id) return;

        var parentPath = parentBaseId == 0 ? "" : PathOfId(list, parentBaseId);
        var path = string.IsNullOrWhiteSpace(parentPath) ? $"n{id}" : $"{parentPath}.n{id}";
        list.Add(new ImportCategoryDto(id, name, path));
    }

    static string PathOfId(List<ImportCategoryDto> list, int id)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Id == id) return list[i].Path;
        return "";
    }
}
