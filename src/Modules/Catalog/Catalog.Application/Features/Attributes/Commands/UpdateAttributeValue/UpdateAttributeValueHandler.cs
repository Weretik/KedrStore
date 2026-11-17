using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.Attributes.Commands.UpdateAttributeValue;

public sealed class UpdateAttributeValueHandler(
    ICatalogRepository<CatalogAttribute> attributes,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<UpdateAttributeValueCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(UpdateAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        var attributeId = AttributeId.Create(command.AttributeId);
        var attribute = await attributes.FirstOrDefaultAsync(new AttributeWithValuesSpec(attributeId), cancellationToken);
        if (attribute is null)
            return Result.NotFound();

        var request = command.Request;
        var edited = attribute.EditValue(AttributeValueId.Create(command.ValueId), request.NameUk, request.NameRu,
            request.SortOrder, request.SwatchHex, request.SwatchMediaFileId, clock.GetUtcNow());
        if (edited.IsFailure)
            return edited.Error!.ToResult<AdminCatalogAttributeDto>();
        await attributes.SaveChangesAsync(cancellationToken);
        return Result.Success((await reader.ReadAsync(attributeId, cancellationToken))!);
    }
}
