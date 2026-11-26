using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.CategoryPlacement.Shared;

/// <summary>
/// What every change of the tree by hand shares (spec catalog/011): the whole tree tracked, the checks of a new parent,
/// rebuilding the paths after a change, and the answer.
/// </summary>
public sealed class CategoryTreeEditor(
    ICatalogRepository<ProductCategory> categories,
    IReadCatalogDbContext db,
    IProductMediaLibrary media)
{
    /// <summary>Every category, deleted ones included, tracked; the tree has a few hundred rows.</summary>
    public async Task<IReadOnlyList<ProductCategory>> LoadTreeAsync(CancellationToken cancellationToken)
        => await categories.ListAsync(new WholeTreeSpec(), cancellationToken);

    /// <summary>
    /// A new parent must be an active category without site models: a model sits only in a final category (spec 007
    /// M-03), so a category with models cannot get subcategories.
    /// </summary>
    public async Task<Result?> CheckParentAsync(ProductCategory? parent, CancellationToken cancellationToken)
    {
        if (parent is null)
            return null;
        if (parent.IsDeleted)
            return Invalid("parentId", CategoryErrors.ParentDeleted().Message, "Parent.Deleted");
        var hasModels = await db.ProductGroups.AnyAsync(model => model.CategoryId == parent.Id, cancellationToken);
        return hasModels
            ? Invalid("parentId", "У цій категорії є моделі: спершу перенесіть їх в іншу категорію.", "Parent.HasModels")
            : null;
    }

    /// <summary>Rebuilds every path from the parents and saves once (MC-03).</summary>
    public async Task SaveAsync(IReadOnlyCollection<ProductCategory> tree, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        CategoryTreePlacement.Rebuild(tree, now);
        await categories.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminCategoryResponse> AnswerAsync(ProductCategory category, CancellationToken cancellationToken)
        => CategoryReadMapper.ToAdminResponse(category,
            await CategoryShareImage.UrlAsync(media, category.ShareImageMediaFileId, cancellationToken));

    public static Result Invalid(string field, string message, string code)
        => Result.Invalid(new ValidationError(field, message, $"Catalog.Category.{code}", ValidationSeverity.Error));

    private sealed class WholeTreeSpec : Specification<ProductCategory>
    {
        public WholeTreeSpec() => Query.IgnoreQueryFilters();
    }
}
