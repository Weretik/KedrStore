using Catalog.Application.Attributes.Recognition;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Application.Jobs.Attributes;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.DecideCode;

public sealed class DecideAttributeCodeCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeAlias> aliases,
    ICatalogRepository<AttributeAliasCategoryValue> categoryValues,
    UnresolvedCodeReader reader,
    RecognizeCatalogAttributesJob recognizer,
    TimeProvider clock,
    ILogger<DecideAttributeCodeCommandHandler> logger)
    : ICommandHandler<DecideAttributeCodeCommand, Result<DecideAttributeCodeResponse>>
{
    public async ValueTask<Result<DecideAttributeCodeResponse>> Handle(DecideAttributeCodeCommand command,
        CancellationToken cancellationToken)
    {
        var aliasId = AttributeAliasId.Create(command.AliasId);
        var alias = await db.AttributeAliases.FirstOrDefaultAsync(item => item.Id == aliasId, cancellationToken);
        if (alias is null)
            return Result.NotFound();

        var request = command.Request;
        AttributeValueId? valueId = request.ValueId is { } id && request.ApplyToAll ? AttributeValueId.Create(id) : null;
        var ownValues = (request.Categories ?? [])
            .Select(item => (CategoryId: ProductCategoryId.From(item.CategoryId),
                ValueId: item.ValueId is { } own ? AttributeValueId.Create(own) : (AttributeValueId?)null))
            .ToList();

        var valueIds = ownValues.Where(item => item.ValueId is not null).Select(item => item.ValueId!.Value).ToList();
        if (valueId is not null)
            valueIds.Add(valueId.Value);
        valueIds = valueIds.Distinct().ToList();
        var known = await db.AttributeValues
            .CountAsync(value => valueIds.Contains(value.Id) && value.AttributeId == alias.AttributeId, cancellationToken);
        if (known != valueIds.Count)
            return AttributeValidation.Invalid("valueId", "The value is not in the code's attribute.", "ValueNotFound");

        var categoryIds = ownValues.Select(item => item.CategoryId).ToList();
        var knownCategories = await db.Categories.IgnoreQueryFilters()
            .CountAsync(category => categoryIds.Contains(category.Id), cancellationToken);
        if (knownCategories != categoryIds.Count)
            return AttributeValidation.Invalid("categories", "A category is not found.", "CategoryNotFound");

        var now = clock.GetUtcNow();
        if (request.ApplyToAll)
            alias.Decide(valueId, now);
        if (request.Categories is not null)
            await ReplaceCategoryValuesAsync(aliasId, ownValues, now, cancellationToken);

        // A product value stays unconfirmed while any code that leads to it is unconfirmed.
        var confirmed = 0;
        if (request.ApplyToAll && valueId is not null && !await db.AttributeAliases.AnyAsync(other => other.Id != aliasId
                && other.AttributeId == alias.AttributeId && other.ValueId == valueId && !other.IsConfirmed,
                cancellationToken))
        {
            var waiting = await db.ProductAttributeValues
                .Where(value => value.AttributeId == alias.AttributeId && value.ValueId == valueId && !value.IsConfirmed)
                .ToListAsync(cancellationToken);
            confirmed = waiting.Count(value => value.Confirm(now));
        }

        await aliases.SaveChangesAsync(cancellationToken);
        await RecognizeAgainAsync(alias.RawValue, cancellationToken);
        return Result.Success(new DecideAttributeCodeResponse((await reader.ReadAsync(aliasId, cancellationToken))!,
            confirmed));
    }

    /// <summary>The code's own decodings per category become exactly the listed ones (spec catalog/008).</summary>
    private async Task ReplaceCategoryValuesAsync(AttributeAliasId aliasId,
        IReadOnlyList<(ProductCategoryId CategoryId, AttributeValueId? ValueId)> wanted, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.AttributeAliasCategoryValues.Where(item => item.AliasId == aliasId)
            .ToListAsync(cancellationToken);
        var removed = existing.Where(item => wanted.All(want => want.CategoryId != item.CategoryId)).ToList();
        foreach (var item in existing.Except(removed))
            item.Change(wanted.First(want => want.CategoryId == item.CategoryId).ValueId, now);
        var created = wanted.Where(want => existing.All(item => item.CategoryId != want.CategoryId))
            .Select(want => AttributeAliasCategoryValue.Create(aliasId, want.CategoryId, want.ValueId, now))
            .ToList();

        if (removed.Count > 0)
            await categoryValues.DeleteRangeAsync(removed, cancellationToken);
        if (created.Count > 0)
            await categoryValues.AddRangeAsync(created, cancellationToken);
    }

    /// <summary>
    /// The decision reaches the products at once: those whose names contain the code are recognized again. A failure
    /// here keeps the decision; the next 1C sync recognizes them.
    /// </summary>
    private async Task RecognizeAgainAsync(string rawValue, CancellationToken cancellationToken)
    {
        try
        {
            var names = await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => new { product.Id, product.Name })
                .ToListAsync(cancellationToken);
            var ids = names.Where(product => ProductAttributeRecognizer.Mentions(product.Name, rawValue))
                .Select(product => product.Id.Value)
                .ToList();
            if (ids.Count > 0)
                await recognizer.RunAsync(apply: true, ids, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Recognizing products with code {RawValue} after the owner's decision failed.",
                rawValue);
        }
    }
}
