using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Contracts.Category;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

/// <summary>
/// Shows or hides a category on the site (spec catalog/007 R-18). Hiding hides every subcategory too; showing shows
/// every subcategory and every parent, so a shown category is always reachable. Deleted subcategories follow, so a
/// category 1C restores keeps its branch's value. The Cosmos import root is never shown (owner decision 2026-11-01).
/// </summary>
public sealed class SetCategorySiteVisibilityCommandHandler(
    ICatalogRepository<ProductCategory> repository,
    IOptions<RootCategoryId> roots)
    : ICommandHandler<SetCategorySiteVisibilityCommand, Result<CategorySiteVisibilityResponse>>
{
    public async ValueTask<Result<CategorySiteVisibilityResponse>> Handle(SetCategorySiteVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesForVisibilitySpec(), cancellationToken);
        var selected = categories.FirstOrDefault(category => category.Id.Value == command.CategoryId && !category.IsDeleted);
        if (selected is null)
            return Result.NotFound();

        if (command.IsShownOnSite &&
            string.Equals(selected.ProductTypeIdOneC, roots.Value.CosmosRootCategoryId, StringComparison.Ordinal))
        {
            return Result.Invalid(new ValidationError("isShownOnSite", "The Cosmos categories are never shown on the site.",
                "Catalog.Category.CosmosHidden", ValidationSeverity.Error));
        }

        var prefix = selected.Path.Value + ".";
        var affected = categories
            .Where(category => category.Path.Value.StartsWith(prefix, StringComparison.Ordinal))
            .Prepend(selected)
            .ToList();
        if (command.IsShownOnSite)
            affected.AddRange(Ancestors(selected, categories.Where(category => !category.IsDeleted)
                .ToDictionary(category => category.Id)));

        var now = DateTimeOffset.UtcNow;
        var changed = affected.Distinct().Count(category => category.SetShownOnSite(command.IsShownOnSite, now));
        if (changed > 0)
            await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(new CategorySiteVisibilityResponse(selected.Id.Value, command.IsShownOnSite, changed));
    }

    private static IEnumerable<ProductCategory> Ancestors(ProductCategory category,
        IReadOnlyDictionary<ProductCategoryId, ProductCategory> active)
    {
        var visited = new HashSet<ProductCategoryId> { category.Id };
        var current = category;
        while (current.ParentId is { } parentId && visited.Add(parentId) && active.TryGetValue(parentId, out var parent))
        {
            yield return parent;
            current = parent;
        }
    }
}

/// <summary>Every category, deleted ones included, tracked for the change.</summary>
internal sealed class AllCategoriesForVisibilitySpec : Specification<ProductCategory>
{
    public AllCategoriesForVisibilitySpec() => Query.IgnoreQueryFilters();
}
