using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.SetCategoryFilters;

public sealed class SetCategoryAttributeFiltersCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<CategoryAttribute> filters,
    CategoryFilterReader reader,
    TimeProvider clock)
    : ICommandHandler<SetCategoryAttributeFiltersCommand, Result<List<AdminCategoryAttributeFilterDto>>>
{
    public async ValueTask<Result<List<AdminCategoryAttributeFilterDto>>> Handle(
        SetCategoryAttributeFiltersCommand command, CancellationToken cancellationToken)
    {
        var categoryId = ProductCategoryId.From(command.CategoryId);
        if (!await db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken))
            return Result.NotFound();

        var requested = command.Request.Filters;
        var attributeIds = requested.Select(filter => AttributeId.Create(filter.AttributeId)).ToList();
        if (await db.Attributes.CountAsync(attribute => attributeIds.Contains(attribute.Id), cancellationToken)
            != attributeIds.Count)
        {
            return AttributeValidation.Invalid("filters", "An attribute was not found.", "NotFound");
        }

        var rows = await db.CategoryAttributes
            .Where(row => row.CategoryId == categoryId)
            .ToDictionaryAsync(row => row.AttributeId, cancellationToken);
        var now = clock.GetUtcNow();
        var created = new List<CategoryAttribute>();
        foreach (var filter in requested)
        {
            var attributeId = AttributeId.Create(filter.AttributeId);
            if (rows.TryGetValue(attributeId, out var row))
                row.EditManually(filter.IsFilterable, filter.SortOrder, now);
            else
                created.Add(CategoryAttribute.CreateManual(categoryId, attributeId, filter.IsFilterable, filter.SortOrder, now));
        }

        if (created.Count > 0)
            await filters.AddRangeAsync(created, cancellationToken);
        await filters.SaveChangesAsync(cancellationToken);
        return Result.Success((await reader.ReadAsync(categoryId, cancellationToken))!);
    }
}
