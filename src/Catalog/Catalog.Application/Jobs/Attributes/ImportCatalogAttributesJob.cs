using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Slugs;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Jobs.Attributes;

/// <summary>
/// Loads the attribute dictionary and the recognizer CSV into the catalog. Idempotent: a second run with the same
/// files changes nothing. Values set by a person or locked are never touched. Without <c>apply</c> it only reports.
/// Original product names are never changed.
/// </summary>
public sealed class ImportCatalogAttributesJob(
    IReadCatalogDbContext db,
    ICatalogRepository<CatalogAttribute> attributes,
    ICatalogRepository<AttributeValue> values,
    ICatalogRepository<AttributeAlias> aliases,
    ICatalogRepository<ProductAttributeValue> productValues,
    ICatalogRepository<ProductAttributeReview> reviews,
    ICatalogRepository<CategoryAttribute> categoryAttributes,
    TimeProvider clock,
    ILogger<ImportCatalogAttributesJob> logger)
{
    public async Task<AttributeImportReport> RunAsync(Stream dictionaryFile, TextReader csvFile, bool apply,
        CancellationToken cancellationToken)
    {
        var report = new AttributeImportReport { Applied = apply };
        var now = clock.GetUtcNow();

        var dictionary = AttributeDictionaryFile.Read(dictionaryFile);
        var csv = AttributeCsvFile.Read(csvFile);
        report.Errors.AddRange(csv.Errors);
        report.ProductsInFile = csv.Products.Count;

        var attributeByCode = await SyncAttributesAsync(dictionary.Attributes, apply, now, report, cancellationToken);
        var valueByKey = await SyncValuesAsync(dictionary.Values.Concat(csv.SizeValues).ToList(), attributeByCode,
            apply, now, report, cancellationToken);
        await SyncAliasesAsync(dictionary.Aliases, attributeByCode, valueByKey, apply, now, report, cancellationToken);
        var categoryByProduct = await SyncProductsAsync(csv.Products, attributeByCode, valueByKey, apply, now, report,
            cancellationToken);
        await SeedCategoryAttributesAsync(csv.Products, categoryByProduct, attributeByCode, apply, now, report,
            cancellationToken);

        logger.LogInformation(
            "Attribute import applied={Applied}: attributes {Attributes}; values {Values}; aliases {Aliases}; " +
            "product values {ProductValues}; reviews {Reviews}; category filters {CategoryAttributes}; errors {Errors}.",
            apply, report.Attributes, report.Values, report.Aliases, report.ProductValues, report.Reviews,
            report.CategoryAttributes, report.Errors.Count);
        return report;
    }

    private async Task<Dictionary<string, CatalogAttribute>> SyncAttributesAsync(
        IReadOnlyList<AttributeDefinition> definitions, bool apply, DateTimeOffset now, AttributeImportReport report,
        CancellationToken cancellationToken)
    {
        var existing = await db.Attributes.ToListAsync(cancellationToken);
        var byCode = existing.ToDictionary(attribute => attribute.Code, StringComparer.Ordinal);
        var usedSlugs = existing.Select(attribute => attribute.Slug).ToHashSet(StringComparer.Ordinal);
        var created = new List<CatalogAttribute>();

        foreach (var definition in definitions)
        {
            if (byCode.TryGetValue(definition.Code, out var attribute))
            {
                Count(report.Attributes, attribute.IsEditedManually,
                    attribute.ApplyDictionary(definition.NameUk, definition.Kind, definition.Unit, definition.SortOrder, now));
                continue;
            }

            var slug = UniqueSlug(
                definition.Slug.Length > 0 ? definition.Slug : SlugText.FromName(definition.NameUk, "vlastyvist"),
                usedSlugs);
            attribute = CatalogAttribute.Create(definition.Code, definition.NameUk, definition.Kind, definition.Unit,
                slug, definition.IsFilterable, definition.IsVariantAxis, definition.SortOrder, now);
            byCode[definition.Code] = attribute;
            created.Add(attribute);
            report.Attributes.Created++;
        }

        if (apply)
        {
            if (created.Count > 0)
                await attributes.AddRangeAsync(created, cancellationToken);
            await attributes.SaveChangesAsync(cancellationToken);
        }

        return byCode;
    }

    private async Task<Dictionary<(string Attribute, string Code), AttributeValue>> SyncValuesAsync(
        IReadOnlyList<AttributeValueDefinition> definitions, Dictionary<string, CatalogAttribute> attributeByCode,
        bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
    {
        var codeById = attributeByCode.Values.Where(attribute => !attribute.IsTransient())
            .ToDictionary(attribute => attribute.Id, attribute => attribute.Code);
        var existing = await db.AttributeValues.ToListAsync(cancellationToken);
        var byKey = existing
            .Where(value => codeById.ContainsKey(value.AttributeId))
            .ToDictionary(value => (codeById[value.AttributeId], value.Code));
        var slugsByAttribute = byKey.GroupBy(pair => pair.Key.Item1, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(pair => pair.Value.Slug).ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);
        var created = new List<AttributeValue>();

        foreach (var definition in definitions)
        {
            if (!attributeByCode.TryGetValue(definition.AttributeCode, out var attribute))
            {
                report.Errors.Add($"Value {definition.Code}: unknown attribute {definition.AttributeCode}.");
                continue;
            }

            var key = (definition.AttributeCode, definition.Code);
            if (byKey.TryGetValue(key, out var value))
            {
                Count(report.Values, value.IsEditedManually,
                    value.ApplyDictionary(definition.NameUk, definition.SortOrder, now));
                continue;
            }

            if (!slugsByAttribute.TryGetValue(definition.AttributeCode, out var usedSlugs))
                slugsByAttribute[definition.AttributeCode] = usedSlugs = new HashSet<string>(StringComparer.Ordinal);

            var slug = UniqueSlug(SlugText.FromName(definition.NameUk, "znachennia"), usedSlugs);
            value = AttributeValue.Create(attribute.Id, definition.Code, definition.NameUk, slug, definition.SortOrder, now);
            byKey[key] = value;
            created.Add(value);
            report.Values.Created++;
        }

        if (apply)
        {
            if (created.Count > 0)
                await values.AddRangeAsync(created, cancellationToken);
            await values.SaveChangesAsync(cancellationToken);
        }

        return byKey;
    }

    private async Task SyncAliasesAsync(IReadOnlyList<AttributeAliasDefinition> definitions,
        Dictionary<string, CatalogAttribute> attributeByCode,
        Dictionary<(string Attribute, string Code), AttributeValue> valueByKey,
        bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
    {
        var codeById = attributeByCode.Values.Where(attribute => !attribute.IsTransient())
            .ToDictionary(attribute => attribute.Id, attribute => attribute.Code);
        var existing = (await db.AttributeAliases.ToListAsync(cancellationToken))
            .Where(alias => codeById.ContainsKey(alias.AttributeId))
            .ToDictionary(alias => (codeById[alias.AttributeId], alias.Scope, alias.RawValue));
        var created = new List<AttributeAlias>();

        foreach (var definition in definitions)
        {
            if (!attributeByCode.TryGetValue(definition.AttributeCode, out var attribute))
            {
                report.Errors.Add($"Alias {definition.RawValue}: unknown attribute {definition.AttributeCode}.");
                continue;
            }

            AttributeValueId? valueId = null;
            if (definition.ValueCode is not null)
            {
                if (!valueByKey.TryGetValue((definition.AttributeCode, definition.ValueCode), out var value))
                {
                    report.Errors.Add($"Alias {definition.RawValue}: unknown value {definition.AttributeCode}/{definition.ValueCode}.");
                    continue;
                }

                valueId = value.Id;
            }

            var key = (definition.AttributeCode, definition.Scope, definition.RawValue.Trim());
            if (existing.TryGetValue(key, out var alias))
            {
                Count(report.Aliases, alias.IsEditedManually,
                    alias.ApplyDictionary(valueId, definition.IsConfirmed, definition.Note, now));
                continue;
            }

            alias = AttributeAlias.Create(attribute.Id, definition.Scope, definition.RawValue, valueId,
                definition.IsConfirmed, definition.Note, now);
            existing[key] = alias;
            created.Add(alias);
            report.Aliases.Created++;
        }

        if (apply)
        {
            if (created.Count > 0)
                await aliases.AddRangeAsync(created, cancellationToken);
            await aliases.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Dictionary<int, ProductCategoryId>> SyncProductsAsync(IReadOnlyList<RecognizedProduct> products,
        Dictionary<string, CatalogAttribute> attributeByCode,
        Dictionary<(string Attribute, string Code), AttributeValue> valueByKey,
        bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
    {
        // Deleted products too: the owner wants attributes kept for every product (2026-11-05).
        var categoryByProduct = await db.Products.IgnoreQueryFilters().AsNoTracking()
            .Select(product => new { product.Id, product.CategoryId })
            .ToDictionaryAsync(product => product.Id.Value, product => product.CategoryId, cancellationToken);
        var existingValues = (await db.ProductAttributeValues.ToListAsync(cancellationToken))
            .ToLookup(value => value.ProductId.Value);
        var existingReviews = await db.ProductAttributeReviews
            .ToDictionaryAsync(review => review.ProductId.Value, cancellationToken);

        var createdValues = new List<ProductAttributeValue>();
        var removedValues = new List<ProductAttributeValue>();
        var createdReviews = new List<ProductAttributeReview>();

        foreach (var recognized in products)
        {
            if (!categoryByProduct.ContainsKey(recognized.ProductId))
            {
                report.Errors.Add($"Line {recognized.LineNumber}: product {recognized.ProductId} is not in the catalog.");
                continue;
            }

            var productId = ProductId.From(recognized.ProductId);
            var current = existingValues[recognized.ProductId]
                .ToDictionary(value => (value.AttributeId, (int)value.Position));
            var wanted = new HashSet<(AttributeId, int)>();

            foreach (var item in recognized.Values)
            {
                if (!attributeByCode.TryGetValue(item.AttributeCode, out var attribute))
                {
                    report.Errors.Add($"Line {recognized.LineNumber}: unknown attribute {item.AttributeCode}.");
                    continue;
                }

                AttributeValueId? valueId = null;
                if (item.ValueCode is not null)
                {
                    if (!valueByKey.TryGetValue((item.AttributeCode, item.ValueCode), out var value))
                    {
                        report.Errors.Add(
                            $"Line {recognized.LineNumber}: unknown value {item.AttributeCode}/{item.ValueCode}.");
                        continue;
                    }

                    valueId = value.Id;
                }

                wanted.Add((attribute.Id, item.Position));
                if (current.TryGetValue((attribute.Id, item.Position), out var row))
                {
                    Count(report.ProductValues, !row.IsAutomatic,
                        row.ApplyAutomatic(valueId, item.Number, item.IsConfirmed, AttributeValueSource.Import, now));
                    continue;
                }

                createdValues.Add(ProductAttributeValue.CreateAutomatic(productId, attribute.Id, item.Position, valueId,
                    item.Number, item.IsConfirmed, AttributeValueSource.Import, now));
                report.ProductValues.Created++;
            }

            foreach (var stale in current.Values.Where(row => !wanted.Contains((row.AttributeId, row.Position))))
            {
                if (stale.IsAutomatic)
                {
                    removedValues.Add(stale);
                    report.ProductValues.Removed++;
                }
            }

            var reasons = recognized.Reasons.Select(reason => new AttributeReviewReason(reason.Code, reason.Text)).ToList();
            if (existingReviews.TryGetValue(recognized.ProductId, out var review))
            {
                Count(report.Reviews, false, review.ApplyAutomatic(recognized.BaseName, reasons, now));
            }
            else
            {
                review = ProductAttributeReview.Create(productId, recognized.BaseName, reasons, now);
                existingReviews[recognized.ProductId] = review;
                createdReviews.Add(review);
                report.Reviews.Created++;
            }
        }

        report.ProductsNeedingReview = existingReviews.Values.Count(review => review.State == AttributeReviewState.NeedsReview);

        if (apply)
        {
            if (removedValues.Count > 0)
                await productValues.DeleteRangeAsync(removedValues, cancellationToken);
            if (createdValues.Count > 0)
                await productValues.AddRangeAsync(createdValues, cancellationToken);
            if (createdReviews.Count > 0)
                await reviews.AddRangeAsync(createdReviews, cancellationToken);
            await reviews.SaveChangesAsync(cancellationToken);
        }

        return categoryByProduct;
    }

    /// <summary>
    /// Offers a filter in a category when its products have at least two different values of a filterable attribute.
    /// Existing category filters are kept as they are, so Admin changes survive.
    /// </summary>
    private async Task SeedCategoryAttributesAsync(IReadOnlyList<RecognizedProduct> products,
        Dictionary<int, ProductCategoryId> categoryByProduct, Dictionary<string, CatalogAttribute> attributeByCode,
        bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
    {
        var existing = (await db.CategoryAttributes.AsNoTracking().ToListAsync(cancellationToken))
            .Select(row => (row.CategoryId, row.AttributeId))
            .ToHashSet();

        var wanted = products
            .Where(product => categoryByProduct.ContainsKey(product.ProductId))
            .SelectMany(product => product.Values
                .Where(value => value.ValueCode is not null)
                .Select(value => (Category: categoryByProduct[product.ProductId], value.AttributeCode, value.ValueCode)))
            .Where(item => attributeByCode.TryGetValue(item.AttributeCode, out var attribute) && attribute.IsFilterable)
            .GroupBy(item => (item.Category, item.AttributeCode))
            .Where(group => group.Select(item => item.ValueCode).Distinct(StringComparer.Ordinal).Count() >= 2)
            .Select(group => (group.Key.Category, Attribute: attributeByCode[group.Key.AttributeCode]))
            .OrderBy(item => item.Category.Value).ThenBy(item => item.Attribute.SortOrder)
            .ToList();

        var created = new List<CategoryAttribute>();
        foreach (var (category, attribute) in wanted)
        {
            if (!attribute.IsTransient() && existing.Contains((category, attribute.Id)))
            {
                report.CategoryAttributes.Unchanged++;
                continue;
            }

            created.Add(CategoryAttribute.Create(category, attribute.Id, true, attribute.SortOrder, now));
            report.CategoryAttributes.Created++;
        }

        if (apply && created.Count > 0)
            await categoryAttributes.AddRangeAsync(created, cancellationToken);
    }

    private static void Count(AttributeImportCounters counters, bool skipped, bool changed)
    {
        if (skipped)
            counters.Skipped++;
        else if (changed)
            counters.Updated++;
        else
            counters.Unchanged++;
    }

    private static string UniqueSlug(string baseSlug, HashSet<string> used)
    {
        var slug = baseSlug;
        for (var number = 2; !used.Add(slug); number++)
            slug = SlugText.WithSuffix(baseSlug, number);

        return slug;
    }
}
