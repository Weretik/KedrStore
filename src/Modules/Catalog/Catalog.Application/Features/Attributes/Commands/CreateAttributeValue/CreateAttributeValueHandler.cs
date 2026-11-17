using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.Attributes.Commands.CreateAttributeValue;

public sealed class CreateAttributeValueHandler(
    ICatalogRepository<CatalogAttribute> attributes,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<CreateAttributeValueCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(CreateAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        var attributeId = AttributeId.Create(command.AttributeId);
        var attribute = await attributes.FirstOrDefaultAsync(new AttributeWithValuesSpec(attributeId), cancellationToken);
        if (attribute is null)
            return Result.NotFound();

        // A typed slug must be free (the attribute refuses a taken one); a slug made from the name gets a number.
        var request = command.Request;
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? SlugText.FromName(request.NameUk, "znachennia") : request.Slug.Trim();
        var unique = slug;
        for (var number = 2; string.IsNullOrWhiteSpace(request.Slug) && attribute.IsValueSlugTaken(unique); number++)
            unique = SlugText.WithSuffix(slug, number);

        var code = string.IsNullOrWhiteSpace(request.Code) ? unique : request.Code.Trim();
        var value = attribute.AddManualValue(code, request.NameUk, request.NameRu, unique, request.SortOrder,
            request.SwatchHex, request.SwatchMediaFileId, clock.GetUtcNow());
        if (value.IsFailure)
            return value.Error!.ToResult<AdminCatalogAttributeDto>();
        await attributes.SaveChangesAsync(cancellationToken);
        return Result.Success((await reader.ReadAsync(attributeId, cancellationToken))!);
    }
}
