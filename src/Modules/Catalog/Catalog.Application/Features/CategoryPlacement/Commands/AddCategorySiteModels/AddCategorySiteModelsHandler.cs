using BuildingBlocks.Application.Users;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.CategoryPlacement.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.CategoryPlacement.Commands.AddCategorySiteModels;

/// <summary>
/// «Додати моделі»: puts several models into one final category in one transaction, each by spec 007 R-22 (a proposed
/// model is accepted first, so the next build keeps it there) (MC-07).
/// </summary>
public sealed class AddCategorySiteModelsHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    ICurrentUserService currentUser,
    SiteModelChanges changes,
    TimeProvider clock,
    ICatalogTransaction transaction)
    : ICommandHandler<AddCategorySiteModelsCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(AddCategorySiteModelsCommand command, CancellationToken cancellationToken)
        => await transaction.RunAsync(ct => HandleInTransactionAsync(command, ct), cancellationToken);

    private async Task<Result<bool>> HandleInTransactionAsync(AddCategorySiteModelsCommand command,
        CancellationToken cancellationToken)
    {
        var categoryId = ProductCategoryId.Create(command.CategoryId);
        if (!await db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken))
            return Result.NotFound();
        if (!await changes.IsFinalCategoryAsync(categoryId, cancellationToken))
        {
            return CategoryTreeEditor.Invalid("categoryId",
                "Моделі можна додати лише в кінцеву категорію (без підкатегорій).", "NotFinal");
        }

        var ids = command.ModelIds.Distinct().Select(ProductGroupId.Create).ToList();
        var found = await db.ProductGroups.Where(model => ids.Contains(model.Id)).ToListAsync(cancellationToken);
        if (found.Count != ids.Count)
            return Result.NotFound("Модель не знайдено.");

        var moving = found.Where(model => model.CategoryId != categoryId).ToList();
        if (moving.Count == 0)
            return Result.Success(true);

        var now = clock.GetUtcNow();
        await changes.AcceptAsync(moving, ReviewerName.Of(currentUser), now, cancellationToken);
        foreach (var model in moving)
            model.ChangeCategory(categoryId, now);
        await models.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
