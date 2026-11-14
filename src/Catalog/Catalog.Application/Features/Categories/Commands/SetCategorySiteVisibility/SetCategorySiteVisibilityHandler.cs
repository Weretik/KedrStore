using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

/// <summary>
/// Shows or hides a category on the site (spec catalog/007 R-18). The tree rule lives in
/// <see cref="CategorySiteVisibility"/>; the Cosmos import root is never shown (owner decision 2026-11-01).
/// </summary>
public sealed class SetCategorySiteVisibilityHandler(
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
            return Result.Invalid(new ValidationError("isShownOnSite", "Категорії Cosmos ніколи не показуються на сайті.",
                "Catalog.Category.CosmosHidden", ValidationSeverity.Error));
        }

        var changed = CategorySiteVisibility.Apply(selected, categories, command.IsShownOnSite, DateTimeOffset.UtcNow);
        if (changed > 0)
            await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(new CategorySiteVisibilityResponse(selected.Id.Value, command.IsShownOnSite, changed));
    }
}

/// <summary>Every category, deleted ones included, tracked for the change.</summary>
internal sealed class AllCategoriesForVisibilitySpec : Specification<ProductCategory>
{
    public AllCategoriesForVisibilitySpec() => Query.IgnoreQueryFilters();
}
