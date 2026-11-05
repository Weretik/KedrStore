using Catalog.Application.Jobs.Attributes;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.Extensions.Logging.Abstractions;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.Attributes;

/// <summary>Runs the attribute import against an in-memory catalog.</summary>
internal static class AttributeImportTestSupport
{
    public static ImportCatalogAttributesJob Job(CatalogDbContext db)
        => new(
            db,
            new TestRepository<CatalogAttribute>(db),
            new TestRepository<AttributeValue>(db),
            new TestRepository<AttributeAlias>(db),
            new TestRepository<ProductAttributeValue>(db),
            new TestRepository<ProductAttributeReview>(db),
            new TestRepository<CategoryAttribute>(db),
            TimeProvider.System,
            NullLogger<ImportCatalogAttributesJob>.Instance);

    public static async Task<AttributeImportReport> RunAsync(CatalogDbContext db, string dictionaryJson, string csv,
        bool apply)
    {
        await using var dictionary = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(dictionaryJson));
        using var reader = new StringReader(csv);
        var report = await Job(db).RunAsync(dictionary, reader, apply, CancellationToken.None);
        db.ChangeTracker.Clear();
        return report;
    }

    public static void AddCategory(CatalogDbContext db, int id)
        => db.Categories.Add(ProductCategory.Create(ProductCategoryId.From(id), Root, $"Категорія {id}", $"katehoriia-{id}",
            CategoryPath.From($"n{id}")));

    public static void AddProduct(CatalogDbContext db, int id, int categoryId, string name)
        => db.Products.Add(Product.Create(ProductId.From(id), Root, name, $"product-{id}", ProductCategoryId.From(categoryId),
            DateTimeOffset.UtcNow.AddDays(-1), 0, 1, false, false, true));

    /// <summary>The repository root, where <c>docs/catalog-attributes</c> lives.</summary>
    public static string RepositoryFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Cannot find {relativePath} above {AppContext.BaseDirectory}.");
    }

    public const string Dictionary = """
        {
          "format_version": "1.0",
          "attribute_labels_uk": { "color": "Колір / декор", "side": "Сторона", "branding": "Маркування бренду",
                                   "material_code": "Нерозшифрований матеріал / покриття" },
          "canonical_values": [
            { "attribute": "color", "code": "AB", "name_uk": "Антична бронза" },
            { "attribute": "color", "code": "CR", "name_uk": "Хром" },
            { "attribute": "side", "code": "L", "name_uk": "Ліва" },
            { "attribute": "side", "code": "R", "name_uk": "Права" },
            { "attribute": "branding", "code": "NO_NAME", "name_uk": "Без маркування бренду" }
          ],
          "entries": [
            { "attribute": "color", "scope": "general", "raw_value": "AB", "normalized_code": "AB", "status": "approved" },
            { "attribute": "color", "scope": "general", "raw_value": "CP", "normalized_code": "CR", "status": "approved",
              "selected_by": "assistant", "user_confirmed": false },
            { "attribute": "material_code", "scope": "general", "raw_value": "AL", "normalized_code": null,
              "status": "unresolved", "note": "Не розшифровано" }
          ]
        }
        """;

    public const string CsvHeader =
        "product_id,category_id,category_path,original_name,base_name,size,size_length,size_width,size_thickness," +
        "size_diameter,color_raw,color_code,color_name,color_secondary_code,color_secondary_name,side," +
        "extra_attributes,needs_review,review_reason,notes";

    public static string Row(int id, string baseName, string color, string side, bool noName = false,
        string? reasonCode = null, string size = "null")
    {
        var attributes = noName ? "\"\"branding\"\":[{\"\"code\"\":\"\"NO_NAME\"\",\"\"scope\"\":\"\"general\"\"}]" : "";
        var reasons = reasonCode is null ? "" : $"\"\"{reasonCode}\"\"";
        var primary = reasonCode is null ? "null" : $"\"\"{reasonCode}\"\"";
        var colorMention = color == "CR"
            ? "[{\"\"raw\"\":\"\"CP\"\",\"\"code\"\":\"\"CR\"\",\"\"selected_by\"\":\"\"assistant\"\"}]"
            : "[]";
        var extra = $"\"{{\"\"attributes\"\":{{{attributes}}},\"\"color_mentions\"\":{colorMention}," +
                    $"\"\"review_reason_codes\"\":[{reasons}],\"\"primary_review_reason_code\"\":{primary}}}\"";
        var needsReview = reasonCode is null ? "false" : "true";
        var reasonText = reasonCode is null ? "null" : "\"Код, не розшифровано\"";
        return $"{id},10,Категорія,\"Назва {id}, з комою\",{baseName},{size},null,null,null,null,{color},{color},x," +
               $"null,null,{side},{extra},{needsReview},{reasonText},";
    }
}
