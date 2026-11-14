using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Attributes.Shared;

/// <summary>
/// Reads «Не розшифровані коди»: aliases without a value or with an unconfirmed decoding that the owner has not decided
/// yet. A product counts for a code when it is on the site, not deleted, and its review mentions the code in
/// «guillemets», as the recognizer writes it (spec catalog/007 M-01).
/// </summary>
public sealed class UnresolvedCodeReader(IReadCatalogDbContext db, CodeMeaningReader meanings)
{
    public async Task<List<AdminUnresolvedCodeDto>> ListAsync(CancellationToken cancellationToken)
    {
        var aliases = await db.AttributeAliases.AsNoTracking()
            .Where(alias => (alias.ValueId == null || !alias.IsConfirmed) && !alias.IsEditedManually)
            .ToListAsync(cancellationToken);
        return await ToDtosAsync(aliases, cancellationToken);
    }

    public async Task<AdminUnresolvedCodeDto?> ReadAsync(AttributeAliasId aliasId, CancellationToken cancellationToken)
    {
        var alias = await db.AttributeAliases.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == aliasId, cancellationToken);
        return alias is null ? null : (await ToDtosAsync([alias], cancellationToken)).Single();
    }

    private async Task<List<AdminUnresolvedCodeDto>> ToDtosAsync(List<AttributeAlias> aliases,
        CancellationToken cancellationToken)
    {
        var attributeNames = await db.Attributes.AsNoTracking()
            .ToDictionaryAsync(attribute => attribute.Id, attribute => attribute.NameUk, cancellationToken);
        var valueIds = aliases.Where(alias => alias.ValueId is not null).Select(alias => alias.ValueId!.Value).ToList();
        var valueNames = await db.AttributeValues.AsNoTracking()
            .Where(value => valueIds.Contains(value.Id))
            .ToDictionaryAsync(value => value.Id, value => value.NameUk, cancellationToken);
        var onSite = db.Products.AsNoTracking()
            .Where(product => product.ExportToSite)
            .Select(product => product.Id);
        var texts = (await db.ProductAttributeReviews.AsNoTracking()
                .Where(review => review.State != AttributeReviewState.Clear && onSite.Contains(review.ProductId))
                .Select(review => review.Reasons)
                .ToListAsync(cancellationToken))
            .Select(reasons => string.Join('\n', reasons.Select(reason => reason.Text)))
            .ToList();

        var meaningsByCode = await meanings.ReadAsync(aliases.Select(alias => alias.Id).ToList(), cancellationToken);

        return aliases
            .OrderBy(alias => attributeNames.GetValueOrDefault(alias.AttributeId))
            .ThenBy(alias => alias.RawValue)
            .Select(alias => new AdminUnresolvedCodeDto(
                alias.Id.Value,
                alias.AttributeId.Value,
                attributeNames.GetValueOrDefault(alias.AttributeId) ?? string.Empty,
                alias.Scope,
                alias.RawValue,
                alias.ValueId?.Value,
                alias.ValueId is { } id ? valueNames.GetValueOrDefault(id) : null,
                alias.IsConfirmed,
                alias.Note,
                texts.Count(text => text.Contains($"«{alias.RawValue}»", StringComparison.Ordinal)),
                meaningsByCode.GetValueOrDefault((alias.Id.Value, null)) ?? []))
            .ToList();
    }
}
