using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;
using Catalog.Domain.Attributes.Enums;

namespace Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;

/// <summary>
/// Adds an attribute to the shared dictionary (spec catalog/007 M-06). Code and slug are unique among attributes; a slug
/// made from the name gets a number when taken, an explicit taken code or slug is refused. A «Так / Ні» attribute
/// (<c>yesno</c>) is a list that gets the values «Так» and «Ні» at once.
/// </summary>
public sealed class CreateCatalogAttributeHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<CatalogAttribute> attributes,
    ICatalogRepository<AttributeValue> values,
    CatalogAttributeReader reader,
    TimeProvider clock)
    : ICommandHandler<CreateCatalogAttributeCommand, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(CreateCatalogAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var existing = await db.Attributes.AsNoTracking()
            .Select(attribute => new { attribute.Code, attribute.Slug })
            .ToListAsync(cancellationToken);

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Shorten(SlugText.FromName(request.NameUk, "oznaka"), 70)
            : request.Slug.Trim();
        var usedSlugs = existing.Select(attribute => attribute.Slug).ToHashSet(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(request.Slug) && usedSlugs.Contains(slug))
            return AttributeValidation.Invalid("slug", "Ця адреса вже є в іншої властивості.", "SlugTaken");
        var unique = slug;
        for (var number = 2; usedSlugs.Contains(unique); number++)
            unique = SlugText.WithSuffix(slug, number);

        var code = string.IsNullOrWhiteSpace(request.Code) ? Shorten(unique.Replace('-', '_'), CatalogAttribute.CodeMaxLength)
            : request.Code.Trim();
        if (existing.Any(attribute => string.Equals(attribute.Code, code, StringComparison.OrdinalIgnoreCase)))
            return AttributeValidation.Invalid("code", "Цей код вже є в іншої властивості.", "CodeTaken");

        var now = clock.GetUtcNow();
        var kind = request.Kind == "number" ? AttributeKind.Number : AttributeKind.Enum;
        var created = CatalogAttribute.Create(code, request.NameUk, kind, request.Unit, unique, request.IsFilterable,
            request.IsVariantAxis, request.SortOrder, now);
        if (created.IsFailure)
            return created.Error!.ToResult<AdminCatalogAttributeDto>();
        var attribute = created.Value;
        var edited = attribute.EditManually(request.NameUk, request.NameRu, request.IsFilterable, request.SortOrder, now);
        if (edited.IsFailure)
            return edited.Error!.ToResult<AdminCatalogAttributeDto>();
        await attributes.AddAsync(attribute, cancellationToken);
        if (request.Kind == CreateCatalogAttributeCommand.YesNo)
        {
            // Fixed values that always pass the rules.
            await values.AddRangeAsync(
            [
                AttributeValue.CreateManual(attribute.Id, "yes", "Так", "Да", "tak", 1, null, null, now).Value,
                AttributeValue.CreateManual(attribute.Id, "no", "Ні", "Нет", "ni", 2, null, null, now).Value
            ], cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(attribute.Id, cancellationToken))!);
    }

    private static string Shorten(string text, int maxLength)
        => text.Length <= maxLength ? text : text[..maxLength].TrimEnd('-', '_');
}
