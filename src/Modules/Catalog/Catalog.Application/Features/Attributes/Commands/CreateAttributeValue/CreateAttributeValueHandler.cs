using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.CreateAttributeValue;

public sealed class CreateAttributeValueHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeValue> values,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<CreateAttributeValueCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(CreateAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        var attributeId = AttributeId.Create(command.AttributeId);
        if (!await db.Attributes.AnyAsync(item => item.Id == attributeId, cancellationToken))
            return Result.NotFound();

        var request = command.Request;
        var existing = await db.AttributeValues.AsNoTracking()
            .Where(value => value.AttributeId == attributeId)
            .Select(value => new { value.Code, value.Slug })
            .ToListAsync(cancellationToken);

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? SlugText.FromName(request.NameUk, "znachennia") : request.Slug.Trim();
        var usedSlugs = existing.Select(value => value.Slug).ToHashSet(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(request.Slug) && usedSlugs.Contains(slug))
            return AttributeValidation.Invalid("slug", "Ця адреса вже є в іншого значення цієї властивості.", "SlugTaken");
        var unique = slug;
        for (var number = 2; usedSlugs.Contains(unique); number++)
            unique = SlugText.WithSuffix(slug, number);

        var code = string.IsNullOrWhiteSpace(request.Code) ? unique : request.Code.Trim();
        if (existing.Any(value => string.Equals(value.Code, code, StringComparison.OrdinalIgnoreCase)))
            return AttributeValidation.Invalid("code", "Цей код вже є в іншого значення цієї властивості.", "CodeTaken");

        var value = AttributeValue.CreateManual(attributeId, code, request.NameUk, request.NameRu, unique,
            request.SortOrder, request.SwatchHex, request.SwatchMediaFileId, clock.GetUtcNow());
        if (value.IsFailure)
            return value.Error!.ToResult<AdminCatalogAttributeDto>();
        await values.AddAsync(value.Value, cancellationToken);
        return Result.Success((await reader.ReadAsync(attributeId, cancellationToken))!);
    }
}
