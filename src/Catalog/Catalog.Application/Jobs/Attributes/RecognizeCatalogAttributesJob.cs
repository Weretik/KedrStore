using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Attributes.Recognition;
using Catalog.Application.Groups;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Jobs.Attributes;

/// <summary>
/// Recognizes attributes from the current 1C names with the .NET recognizer (spec catalog/006, phase 4) and stores
/// them as parser values: after a 1C sync and after the owner decodes a code. The owner's code decisions take
/// priority over the dictionary file. Values a person set or locked are kept; original names are never changed.
/// Without <c>apply</c> it only reports.
/// </summary>
public sealed class RecognizeCatalogAttributesJob(
    IReadCatalogDbContext db,
    RecognizedProductWriter writer,
    ProductGroupBuilder groupBuilder,
    TimeProvider clock,
    ILogger<RecognizeCatalogAttributesJob> logger)
{
    /// <param name="productIds">Only these products; all products (deleted ones too) when null.</param>
    public async Task<AttributeImportReport> RunAsync(bool apply, IReadOnlyCollection<int>? productIds,
        CancellationToken cancellationToken)
    {
        var report = new AttributeImportReport { Applied = apply };
        var now = clock.GetUtcNow();

        var attributeByCode = (await db.Attributes.ToListAsync(cancellationToken))
            .ToDictionary(attribute => attribute.Code, StringComparer.Ordinal);
        if (attributeByCode.Count == 0)
        {
            report.Errors.Add("No attributes yet: run import-catalog-attributes first.");
            return report;
        }

        var categoryRows = await db.Categories.IgnoreQueryFilters().AsNoTracking()
            .Select(category => new { category.Id, category.ParentId, category.ShortNameUk, category.Name })
            .ToListAsync(cancellationToken);
        var categories = CategoryPaths.Build(categoryRows.Select(category => new RecognitionCategory(
            category.Id.Value, category.ParentId?.Value,
            string.IsNullOrEmpty(category.ShortNameUk) ? category.Name : category.ShortNameUk)));

        await using var file = RecognitionDictionaryFile.Open();
        var dictionary = RecognitionDictionary.Read(file, categories, await DecisionsAsync(cancellationToken));

        var query = db.Products.IgnoreQueryFilters().AsNoTracking();
        if (productIds is not null)
        {
            var ids = productIds.Select(ProductId.From).ToList();
            query = query.Where(product => ids.Contains(product.Id));
        }

        var products = await query
            .Select(product => new { product.Id, product.CategoryId, product.Name })
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);

        var sizes = new Dictionary<string, string>(StringComparer.Ordinal);
        var recognized = new List<RecognizedProduct>(products.Count);
        foreach (var product in products)
        {
            var row = ProductAttributeRecognizer.Recognize(
                new RecognitionProduct(product.Id.Value, product.CategoryId.Value, product.Name), dictionary, categories);
            try
            {
                recognized.Add(AttributeCsvFile.FromCells(product.Id.Value, row.Cell, sizes));
            }
            catch (FormatException exception)
            {
                report.Errors.Add($"Product {product.Id.Value}: {exception.Message}");
            }
        }

        report.ProductsInFile = recognized.Count;
        var valueByKey = await writer.SyncValuesAsync(AttributeCsvFile.ToSizeValues(sizes), attributeByCode,
            updateExisting: false, apply, now, report, cancellationToken);
        await writer.WriteProductsAsync(recognized, attributeByCode, valueByKey, AttributeValueSource.Parser, apply, now,
            report, cancellationToken);
        report.Groups = await groupBuilder.RunAsync(apply, cancellationToken);

        logger.LogInformation(
            "Attribute recognition applied={Applied}: products {Products}; values {Values}; product values {ProductValues}; " +
            "reviews {Reviews}; category filters {CategoryAttributes}; groups {Groups}; errors {Errors}.",
            apply, recognized.Count, report.Values, report.ProductValues, report.Reviews, report.CategoryAttributes,
            report.Groups.Groups, report.Errors.Count);
        return report;
    }

    /// <summary>Codes the owner decoded in «Не розшифровані коди»: they win over the dictionary file.</summary>
    private async Task<List<AttributeCodeDecision>> DecisionsAsync(CancellationToken cancellationToken)
        => await (from alias in db.AttributeAliases.AsNoTracking()
                where alias.IsEditedManually
                join attribute in db.Attributes on alias.AttributeId equals attribute.Id
                join value in db.AttributeValues on alias.ValueId equals value.Id into decided
                from value in decided.DefaultIfEmpty()
                select new AttributeCodeDecision(attribute.Code, alias.Scope, alias.RawValue,
                    value == null ? null : value.Code, value == null ? null : value.NameUk))
            .ToListAsync(cancellationToken);
}
