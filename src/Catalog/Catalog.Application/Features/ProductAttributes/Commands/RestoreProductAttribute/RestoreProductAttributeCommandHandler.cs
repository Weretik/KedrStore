using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.ProductAttributes.Commands.RestoreProductAttribute;

public sealed class RestoreProductAttributeCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductAttributeValue> values,
    ProductGroupBuilder groups,
    ProductAttributesReader reader,
    TimeProvider clock)
    : ICommandHandler<RestoreProductAttributeCommand, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(RestoreProductAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.ProductId);
        if (!await db.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == productId, cancellationToken))
            return Result.NotFound();

        var attributeId = AttributeId.Create(command.AttributeId);
        var attribute = await db.Attributes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == attributeId, cancellationToken);
        if (attribute is null)
            return AttributeValidation.Invalid("attributeId", "Attribute not found.", "NotFound");

        var manual = await db.ProductAttributeValues
            .Where(value => value.ProductId == productId && value.AttributeId == attributeId)
            .ToListAsync(cancellationToken);
        manual = manual.Where(value => !value.IsAutomatic).ToList();
        if (manual.Count == 0)
            return Result.Success((await reader.ReadAsync(productId, cancellationToken))!);

        var now = clock.GetUtcNow();
        var removed = manual.Where(value => !value.RestoreAutomatic(now)).ToList();
        if (removed.Count > 0)
            await values.DeleteRangeAsync(removed, cancellationToken);
        await values.SaveChangesAsync(cancellationToken);

        if (attribute.IsVariantAxis)
            await groups.RunAsync(apply: true, cancellationToken);

        return Result.Success((await reader.ReadAsync(productId, cancellationToken))!);
    }
}
