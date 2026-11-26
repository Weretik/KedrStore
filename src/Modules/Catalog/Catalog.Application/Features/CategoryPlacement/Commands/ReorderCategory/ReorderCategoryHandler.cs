using Catalog.Application.Features.CategoryPlacement.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.ReorderCategory;

/// <summary>
/// «Вище» / «Нижче»: swaps the category with its neighbour in the shown order (sort order, then id) and numbers the
/// neighbours anew, all by hand, so the 1C sync keeps the order (MC-10). At the edge nothing changes.
/// </summary>
public sealed class ReorderCategoryHandler(CategoryTreeEditor editor, TimeProvider clock)
    : ICommandHandler<ReorderCategoryCommand, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(ReorderCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var tree = await editor.LoadTreeAsync(cancellationToken);
        var category = tree.FirstOrDefault(item => item.Id.Value == command.CategoryId && !item.IsDeleted);
        if (category is null)
            return Result.NotFound();

        var siblings = tree.Where(item => !item.IsDeleted && item.ParentId == category.ParentId)
            .OrderBy(item => item.SortOrder).ThenBy(item => item.Id.Value).ToList();
        var index = siblings.IndexOf(category);
        var target = command.Direction == "up" ? index - 1 : index + 1;
        if (target < 0 || target >= siblings.Count)
            return Result.Success(await editor.AnswerAsync(category, cancellationToken));

        (siblings[index], siblings[target]) = (siblings[target], siblings[index]);
        var now = clock.GetUtcNow();
        for (var position = 0; position < siblings.Count; position++)
            siblings[position].SetOrder(position, now);
        await editor.SaveAsync(tree, now, cancellationToken);
        return Result.Success(await editor.AnswerAsync(category, cancellationToken));
    }
}
