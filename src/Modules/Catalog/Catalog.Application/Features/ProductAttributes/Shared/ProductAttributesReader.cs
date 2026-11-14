using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Shared;

/// <summary>Reads the «Властивості» panel of one product: original name, review, and attribute values.</summary>
public sealed class ProductAttributesReader(IReadCatalogDbContext db)
{
    public async Task<AdminProductAttributesDto?> ReadAsync(ProductId productId, CancellationToken cancellationToken)
    {
        var name = await db.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.Id == productId)
            .Select(product => product.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null)
            return null;

        var review = await db.ProductAttributeReviews.AsNoTracking()
            .FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
        var rows = await db.ProductAttributeValues.AsNoTracking()
            .Where(value => value.ProductId == productId)
            .ToListAsync(cancellationToken);
        var attributeIds = rows.Select(row => row.AttributeId).Distinct().ToList();
        var attributes = await db.Attributes.AsNoTracking()
            .Where(attribute => attributeIds.Contains(attribute.Id))
            .ToListAsync(cancellationToken);
        var valueIds = rows.Where(row => row.ValueId is not null).Select(row => row.ValueId!.Value).Distinct().ToList();
        var valueNames = await db.AttributeValues.AsNoTracking()
            .Where(value => valueIds.Contains(value.Id))
            .ToDictionaryAsync(value => value.Id, value => value.NameUk, cancellationToken);

        var items = attributes
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Id.Value)
            .Select(attribute => new AdminProductAttributeDto(
                attribute.Id.Value,
                attribute.Code,
                attribute.NameUk,
                AttributeDtoNames.Kind(attribute.Kind),
                attribute.Unit,
                attribute.IsVariantAxis,
                rows.Where(row => row.AttributeId == attribute.Id)
                    .OrderBy(row => row.Position)
                    .Select(row => new AdminProductAttributeValueDto(
                        row.Position,
                        row.ValueId?.Value,
                        row.ValueId is { } id && valueNames.TryGetValue(id, out var valueName) ? valueName : null,
                        row.NumberValue,
                        AttributeDtoNames.Source(row.Source),
                        row.IsLocked,
                        row.IsConfirmed,
                        row.AutomaticSource is not null))
                    .ToList()))
            .ToList();

        var reviewDto = review is null
            ? new AdminProductReviewDto("clear", [], null, null)
            : new AdminProductReviewDto(
                AttributeDtoNames.State(review.State),
                review.Reasons.Select(reason => new AdminAttributeProblemDto(reason.Code, reason.Text)).ToList(),
                review.AcceptedBy,
                review.AcceptedAt);

        return new AdminProductAttributesDto(productId.Value, name, review?.BaseName, reviewDto, items);
    }
}
