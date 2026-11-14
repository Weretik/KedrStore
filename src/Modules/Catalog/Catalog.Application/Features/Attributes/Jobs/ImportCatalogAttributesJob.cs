using Catalog.Application.Features.Attributes.Shared.Import;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Jobs;

/// <summary>
/// Loads the attribute dictionary and the recognizer CSV into the catalog. Idempotent: a second run with the same
/// files changes nothing. Values set by a person or locked are never touched. Without <c>apply</c> it only reports.
/// Original product names are never changed.
/// </summary>
public sealed class ImportCatalogAttributesJob(
    IReadCatalogDbContext db,
    ICatalogRepository<CatalogAttribute> attributes,
    ICatalogRepository<AttributeAlias> aliases,
    RecognizedProductWriter writer,
    ProductGroupBuilder groupBuilder,
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
        var valueByKey = await writer.SyncValuesAsync(dictionary.Values.Concat(csv.SizeValues).ToList(), attributeByCode,
            updateExisting: true, apply, now, report, cancellationToken);
        await SyncAliasesAsync(dictionary.Aliases, attributeByCode, valueByKey, apply, now, report, cancellationToken);
        await writer.WriteProductsAsync(csv.Products, attributeByCode, valueByKey, AttributeValueSource.Import, apply, now,
            report, cancellationToken);
        // Groups follow the stored base names; a dry run of a first import has none yet, so it reports no groups.
        report.Groups = await groupBuilder.RunAsync(apply, cancellationToken);

        logger.LogInformation(
            "Attribute import applied={Applied}: attributes {Attributes}; values {Values}; aliases {Aliases}; " +
            "product values {ProductValues}; reviews {Reviews}; category filters {CategoryAttributes}; groups {Groups}; " +
            "group members {GroupMembers}; errors {Errors}.",
            apply, report.Attributes, report.Values, report.Aliases, report.ProductValues, report.Reviews,
            report.CategoryAttributes, report.Groups.Groups, report.Groups.Members, report.Errors.Count);
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
                var applied = attribute.ApplyDictionary(definition.NameUk, definition.Kind, definition.Unit,
                    definition.SortOrder, now);
                if (applied.IsFailure)
                    report.Errors.Add($"Attribute {definition.Code}: {applied.Error!.Message}");
                else
                    RecognizedProductWriter.Count(report.Attributes, attribute.IsEditedManually, applied.Value);
                continue;
            }

            var slug = RecognizedProductWriter.UniqueSlug(
                definition.Slug.Length > 0 ? definition.Slug : SlugText.FromName(definition.NameUk, "vlastyvist"),
                usedSlugs);
            var createdAttribute = CatalogAttribute.Create(definition.Code, definition.NameUk, definition.Kind,
                definition.Unit, slug, definition.IsFilterable, definition.IsVariantAxis, definition.SortOrder, now);
            if (createdAttribute.IsFailure)
            {
                report.Errors.Add($"Attribute {definition.Code}: {createdAttribute.Error!.Message}");
                continue;
            }

            attribute = createdAttribute.Value;
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
                var applied = alias.ApplyDictionary(valueId, definition.IsConfirmed, definition.Note, now);
                if (applied.IsFailure)
                    report.Errors.Add($"Alias {definition.RawValue}: {applied.Error!.Message}");
                else
                    RecognizedProductWriter.Count(report.Aliases, alias.IsEditedManually, applied.Value);
                continue;
            }

            var createdAlias = AttributeAlias.Create(attribute.Id, definition.Scope, definition.RawValue, valueId,
                definition.IsConfirmed, definition.Note, now);
            if (createdAlias.IsFailure)
            {
                report.Errors.Add($"Alias {definition.RawValue}: {createdAlias.Error!.Message}");
                continue;
            }

            alias = createdAlias.Value;
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
}
