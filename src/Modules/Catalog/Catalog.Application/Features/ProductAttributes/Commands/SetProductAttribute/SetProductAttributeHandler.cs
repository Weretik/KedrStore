using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;

public sealed class SetProductAttributeHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductAttributeValue> values,
    ProductGroupBuilder groups,
    ProductAttributesReader reader,
    TimeProvider clock)
    : ICommandHandler<SetProductAttributeCommand, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(SetProductAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.ProductId);
        if (!await db.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == productId, cancellationToken))
            return Result.NotFound();

        var attributeId = AttributeId.Create(command.AttributeId);
        var attribute = await db.Attributes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == attributeId, cancellationToken);
        if (attribute is null)
            return AttributeValidation.Invalid("attributeId", "Властивість не знайдено.", "NotFound");

        var requested = command.Request.Values;
        if (attribute.Kind == AttributeKind.Number)
        {
            if (requested.Any(value => value.Number is null))
                return AttributeValidation.Invalid("values", "Ця властивість приймає лише числа.", "NumberRequired");
        }
        else
        {
            if (requested.Any(value => value.ValueId is null))
                return AttributeValidation.Invalid("values", "Ця властивість приймає лише значення зі свого списку.", "ValueRequired");

            var ids = requested.Select(value => AttributeValueId.Create(value.ValueId!.Value)).Distinct().ToList();
            var known = await db.AttributeValues.AsNoTracking()
                .CountAsync(value => ids.Contains(value.Id) && value.AttributeId == attributeId, cancellationToken);
            if (known != ids.Count)
                return AttributeValidation.Invalid("values", "Цього значення немає в списку властивості.", "ValueNotFound");
        }

        var now = clock.GetUtcNow();
        var rows = await db.ProductAttributeValues
            .Where(value => value.ProductId == productId && value.AttributeId == attributeId)
            .ToListAsync(cancellationToken);
        var created = new List<ProductAttributeValue>();
        for (var position = 0; position < requested.Count; position++)
        {
            var item = requested[position];
            AttributeValueId? valueId = item.ValueId is { } id ? AttributeValueId.Create(id) : null;
            var row = rows.FirstOrDefault(value => value.Position == position);
            if (row is null)
            {
                var value = ProductAttributeValue.CreateManual(productId, attributeId, position, valueId, item.Number, now);
                if (value.IsFailure)
                    return value.Error!.ToResult<AdminProductAttributesDto>();
                created.Add(value.Value);
            }
            else
            {
                var set = row.SetManually(valueId, item.Number, now);
                if (set.IsFailure)
                    return set.Error!.ToResult<AdminProductAttributesDto>();
            }
        }

        // Positions beyond the list are dropped; the import adds no positions to a manual attribute.
        var removed = rows.Where(row => row.Position >= requested.Count).ToList();
        if (removed.Count > 0)
            await values.DeleteRangeAsync(removed, cancellationToken);
        if (created.Count > 0)
            await values.AddRangeAsync(created, cancellationToken);
        await values.SaveChangesAsync(cancellationToken);

        // Colors, sizes, and sides decide «Однакові варіанти» of the group.
        if (attribute.IsVariantAxis)
            await groups.RunAsync(apply: true, cancellationToken);

        return Result.Success((await reader.ReadAsync(productId, cancellationToken))!);
    }
}
