using Catalog.Application.Features.CategoryPlacement.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.MoveCategory;

/// <summary>
/// Puts any category, 1C or manual, under another active category without models or at the top; its subcategories go
/// with it, and the 1C sync keeps the place (MC-03, MC-09).
/// </summary>
public sealed class MoveCategoryHandler(CategoryTreeEditor editor, TimeProvider clock)
    : ICommandHandler<MoveCategoryCommand, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(MoveCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var tree = await editor.LoadTreeAsync(cancellationToken);
        var category = tree.FirstOrDefault(item => item.Id.Value == command.CategoryId && !item.IsDeleted);
        if (category is null)
            return Result.NotFound();
        ProductCategory? parent = null;
        if (command.ParentId is { } parentId)
        {
            parent = tree.FirstOrDefault(item => item.Id.Value == parentId);
            if (parent is null)
                return Result.NotFound();
        }

        if (parent?.Id != category.ParentId && await editor.CheckParentAsync(parent, cancellationToken) is { } refused)
            return refused;

        var now = clock.GetUtcNow();
        var moved = category.MoveUnder(parent, now);
        if (moved.IsFailure)
            return moved.Error!.ToResult<AdminCategoryResponse>();
        if (moved.Value is not null)
            await editor.SaveAsync(tree, now, cancellationToken);
        return Result.Success(await editor.AnswerAsync(category, cancellationToken));
    }
}
