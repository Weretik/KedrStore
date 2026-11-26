using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.CategoryPlacement.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.CreateManualCategory;

/// <summary>
/// Creates a category by hand under any active category without models, or at the top (MC-01, MC-02). The same
/// Ukrainian name under the same parent answers 409, which also stops a repeated submit.
/// </summary>
public sealed class CreateManualCategoryHandler(
    ICatalogRepository<ProductCategory> categories,
    CategoryTreeEditor editor,
    IManualCategoryIds ids,
    ISlugService slugs,
    TimeProvider clock)
    : ICommandHandler<CreateManualCategoryCommand, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(CreateManualCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tree = await editor.LoadTreeAsync(cancellationToken);
        ProductCategory? parent = null;
        if (request.ParentId is { } parentId)
        {
            parent = tree.FirstOrDefault(category => category.Id.Value == parentId);
            if (parent is null)
                return Result.NotFound();
        }

        if (await editor.CheckParentAsync(parent, cancellationToken) is { } refused)
            return refused;

        var nameUk = request.NameUk.Trim();
        if (tree.Any(category => !category.IsDeleted && category.ParentId == parent?.Id &&
                                 string.Equals(category.ShortNameUk, nameUk, StringComparison.OrdinalIgnoreCase)))
            return CategoryErrors.NameTaken().ToResult<AdminCategoryResponse>();

        var now = clock.GetUtcNow();
        var allocator = await slugs.LoadAllocatorAsync(CatalogEntityType.Category, cancellationToken);
        var created = ProductCategory.CreateManual(await ids.NextAsync(cancellationToken), nameUk, request.NameRu,
            allocator.Allocate(nameUk), parent, request.IsShownOnSite, now);
        if (created.IsFailure)
            return created.Error!.ToResult<AdminCategoryResponse>();

        await categories.AddAsync(created.Value, cancellationToken);
        await categories.SaveChangesAsync(cancellationToken);
        return Result.Created(await editor.AnswerAsync(created.Value, cancellationToken));
    }
}
