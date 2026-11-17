using Catalog.Application.Features.Attributes.Shared.Recognition;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.Attributes.Jobs;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Attributes.Aliases;

namespace Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;

public sealed class DecideAttributeCodeHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeAlias> aliases,
    UnresolvedCodeReader reader,
    RecognizeCatalogAttributesJob recognizer,
    TimeProvider clock,
    ILogger<DecideAttributeCodeHandler> logger)
    : ICommandHandler<DecideAttributeCodeCommand, Result<DecideAttributeCodeResponse>>
{
    public async ValueTask<Result<DecideAttributeCodeResponse>> Handle(DecideAttributeCodeCommand command,
        CancellationToken cancellationToken)
    {
        var aliasId = AttributeAliasId.Create(command.AliasId);
        var alias = await db.AttributeAliases.Include(item => item.CategoryValues).Include(item => item.Meanings)
            .FirstOrDefaultAsync(item => item.Id == aliasId, cancellationToken);
        if (alias is null)
            return Result.NotFound();

        var request = command.Request;
        var forAll = Split(alias.AttributeId, request.Meanings, request.ValueId);
        var valueId = request.ApplyToAll ? forAll.Own : null;
        var ownValues = (request.Categories ?? [])
            .Select(item => (CategoryId: ProductCategoryId.From(item.CategoryId),
                Decoding: Split(alias.AttributeId, item.Meanings, item.ValueId)))
            .ToList();

        var wanted = ownValues.SelectMany(item => Pairs(alias.AttributeId, item.Decoding)).ToList();
        if (request.ApplyToAll)
            wanted.AddRange(Pairs(alias.AttributeId, forAll));
        wanted = wanted.Distinct().ToList();
        var wantedValueIds = wanted.Select(pair => pair.ValueId).ToList();
        var known = (await db.AttributeValues
                .Where(value => wantedValueIds.Contains(value.Id))
                .Select(value => new { value.Id, value.AttributeId })
                .ToListAsync(cancellationToken))
            .Select(value => (value.AttributeId, ValueId: value.Id))
            .ToHashSet();
        if (wanted.Any(pair => !known.Contains(pair)))
            return AttributeValidation.Invalid("valueId", "Цього значення немає у властивості.", "ValueNotFound");

        var categoryIds = ownValues.Select(item => item.CategoryId).ToList();
        var knownCategories = await db.Categories.IgnoreQueryFilters()
            .CountAsync(category => categoryIds.Contains(category.Id), cancellationToken);
        if (knownCategories != categoryIds.Count)
            return AttributeValidation.Invalid("categories", "A category is not found.", "CategoryNotFound");

        var now = clock.GetUtcNow();
        if (request.ApplyToAll)
        {
            alias.Decide(valueId, now);
            alias.ReplaceMeanings(Rows([(null, forAll.Others)]), forAllProducts: true, now);
        }

        if (request.Categories is not null)
        {
            alias.ReplaceCategoryValues(ownValues.Select(item => (item.CategoryId, item.Decoding.Own)).ToList(), now);
            alias.ReplaceMeanings(
                Rows(ownValues.Select(item => ((ProductCategoryId?)item.CategoryId, item.Decoding.Others)).ToList()),
                forAllProducts: false, now);
        }

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

    /// <summary>
    /// A decoding as the value of the code's own attribute and the values of other attributes (spec catalog/009).
    /// Without meanings it is the older single value of the code's own attribute.
    /// </summary>
    private static Decoding Split(AttributeId ownAttribute, IReadOnlyList<CodeMeaningRequest>? meanings, int? valueId)
    {
        if (meanings is null)
            return new Decoding(valueId is { } id ? AttributeValueId.Create(id) : null, []);

        var pairs = meanings
            .Select(item => (AttributeId: AttributeId.Create(item.AttributeId), ValueId: AttributeValueId.Create(item.ValueId)))
            .ToList();
        return new Decoding(
            pairs.Where(pair => pair.AttributeId == ownAttribute).Select(pair => (AttributeValueId?)pair.ValueId).FirstOrDefault(),
            pairs.Where(pair => pair.AttributeId != ownAttribute).ToList());
    }

    private static IEnumerable<(AttributeId AttributeId, AttributeValueId ValueId)> Pairs(AttributeId ownAttribute,
        Decoding decoding)
        => decoding.Own is { } own ? decoding.Others.Prepend((ownAttribute, own)) : decoding.Others;

    private static List<(ProductCategoryId? CategoryId, AttributeId AttributeId, AttributeValueId ValueId)> Rows(
        IReadOnlyList<(ProductCategoryId? CategoryId, IReadOnlyList<(AttributeId AttributeId, AttributeValueId ValueId)> Pairs)> scopes)
        => scopes.SelectMany(scope => scope.Pairs.Select(pair => (scope.CategoryId, pair.AttributeId, pair.ValueId))).ToList();

    private sealed record Decoding(
        AttributeValueId? Own, IReadOnlyList<(AttributeId AttributeId, AttributeValueId ValueId)> Others);

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
