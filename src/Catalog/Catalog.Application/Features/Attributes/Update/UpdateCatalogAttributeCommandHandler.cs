using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Attributes.Update;

public sealed class UpdateCatalogAttributeCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<CatalogAttribute> attributes,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<UpdateCatalogAttributeCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(UpdateCatalogAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var attributeId = AttributeId.Create(command.AttributeId);
        var attribute = await db.Attributes.FirstOrDefaultAsync(item => item.Id == attributeId, cancellationToken);
        if (attribute is null)
            return Result.NotFound();

        var request = command.Request;
        attribute.EditManually(request.NameUk, request.NameRu, request.IsFilterable, request.SortOrder,
            clock.GetUtcNow());
        await attributes.SaveChangesAsync(cancellationToken);
        return Result.Success((await reader.ReadAsync(attributeId, cancellationToken))!);
    }
}
