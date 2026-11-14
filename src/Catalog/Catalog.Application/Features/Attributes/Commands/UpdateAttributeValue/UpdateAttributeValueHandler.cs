using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Attributes.Commands.UpdateAttributeValue;

public sealed class UpdateAttributeValueHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeValue> values,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<UpdateAttributeValueCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(UpdateAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        var attributeId = AttributeId.Create(command.AttributeId);
        var valueId = AttributeValueId.Create(command.ValueId);
        var value = await db.AttributeValues
            .FirstOrDefaultAsync(item => item.Id == valueId && item.AttributeId == attributeId, cancellationToken);
        if (value is null)
            return Result.NotFound();

        var request = command.Request;
        value.EditManually(request.NameUk, request.NameRu, request.SortOrder, request.SwatchHex,
            request.SwatchMediaFileId, clock.GetUtcNow());
        await values.SaveChangesAsync(cancellationToken);
        return Result.Success((await reader.ReadAsync(attributeId, cancellationToken))!);
    }
}
