using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Jobs;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ProductValues;
using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.Attributes.Shared.Import;

/// <summary>
/// Stores recognition results, shared by the CSV import and the .NET recognizer: attribute values, product values,
/// review state, and category filters. Idempotent; a person's values and edits are never overwritten.
/// </summary>
public sealed class RecognizedProductWriter(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeValue> values,
    ICatalogRepository<ProductAttributeValue> productValues,
    ICatalogRepository<ProductAttributeReview> reviews,
    ICatalogRepository<CategoryAttribute> categoryAttributes)
{
    /// <summary>
    /// Stores attribute values by (attribute code, value code). New ones are created; existing ones take the
    /// dictionary name and order only when <paramref name="updateExisting"/> (the dictionary import), never when a
    /// person edited them.
    /// </summary>
    public async Task<Dictionary<(string Attribute, string Code), AttributeValue>> SyncValuesAsync(
        IReadOnlyList<AttributeValueDefinition> definitions, Dictionary<string, CatalogAttribute> attributeByCode,
        bool updateExisting, bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
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
                if (updateExisting)
                {
                    var applied = value.ApplyDictionary(definition.NameUk, definition.SortOrder, now);
                    if (applied.IsFailure)
                        report.Errors.Add($"Value {definition.Code}: {applied.Error!.Message}");
                    else
                        Count(report.Values, value.IsEditedManually, applied.Value);
                }
                else
                    report.Values.Unchanged++;
                continue;
            }

            if (!slugsByAttribute.TryGetValue(definition.AttributeCode, out var usedSlugs))
                slugsByAttribute[definition.AttributeCode] = usedSlugs = new HashSet<string>(StringComparer.Ordinal);

            var slug = UniqueSlug(SlugText.FromName(definition.NameUk, "znachennia"), usedSlugs);
            var createdValue = AttributeValue.Create(attribute.Id, definition.Code, definition.NameUk, slug,
                definition.SortOrder, now);
            if (createdValue.IsFailure)
            {
                report.Errors.Add($"Value {definition.Code}: {createdValue.Error!.Message}");
                continue;
            }

            value = createdValue.Value;
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

    /// <summary>
    /// Writes recognized products: automatic values from <paramref name="source"/>, review state and base name,
    /// and new category filters. Values a person set or locked are left as they are.
    /// </summary>
    public async Task WriteProductsAsync(IReadOnlyList<RecognizedProduct> products,
        Dictionary<string, CatalogAttribute> attributeByCode,
        Dictionary<(string Attribute, string Code), AttributeValue> valueByKey, AttributeValueSource source,
        bool apply, DateTimeOffset now, AttributeImportReport report, CancellationToken cancellationToken)
    {
        var categoryByProduct = await SyncProductsAsync(products, attributeByCode, valueByKey, source, apply, now, report,
            cancellationToken);
        await SeedCategoryAttributesAsync(products, categoryByProduct, attributeByCode, apply, now, report, cancellationToken);
    }

    private async Task<Dictionary<int, ProductCategoryId>> SyncProductsAsync(IReadOnlyList<RecognizedProduct> products,
        Dictionary<string, CatalogAttribute> attributeByCode,
        Dictionary<(string Attribute, string Code), AttributeValue> valueByKey, AttributeValueSource source,
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
                        row.ApplyAutomatic(valueId, item.Number, item.IsConfirmed, source, now));
                    continue;
                }

                // A person set this attribute by hand: automation adds no positions to it.
                if (current.Values.Any(other => other.AttributeId == attribute.Id && !other.IsAutomatic))
                {
                    report.ProductValues.Skipped++;
                    continue;
                }

                createdValues.Add(ProductAttributeValue.CreateAutomatic(productId, attribute.Id, item.Position, valueId,
                    item.Number, item.IsConfirmed, source, now));
                report.ProductValues.Created++;
            }

            foreach (var stale in current.Values.Where(row => !wanted.Contains((row.AttributeId, row.Position))))
            {
                if (stale.IsAutomatic)
                {
                    removedValues.Add(stale);
                    report.ProductValues.Removed++;
                }
                else
                {
                    stale.ClearAutomatic();
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

    internal static void Count(AttributeImportCounters counters, bool skipped, bool changed)
    {
        if (skipped)
            counters.Skipped++;
        else if (changed)
            counters.Updated++;
        else
            counters.Unchanged++;
    }


    internal static string UniqueSlug(string baseSlug, HashSet<string> used)
    {
        var slug = baseSlug;
        for (var number = 2; !used.Add(slug); number++)
            slug = SlugText.WithSuffix(baseSlug, number);

        return slug;
    }
}
