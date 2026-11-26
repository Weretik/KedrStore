using BuildingBlocks.Application.Users;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.ChangeSiteModelCategory;

/// <summary>
/// Changes a model's category to a final one (R-22). A proposed model is accepted first, so the next build cannot
/// rebuild it in its 1C category; its products keep their 1C categories.
/// </summary>
public sealed class ChangeSiteModelCategoryHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    ICurrentUserService currentUser,
    SiteModelChanges changes,
    SiteModelReader reader,
    TimeProvider clock,
    ICatalogTransaction transaction)
    : ICommandHandler<ChangeSiteModelCategoryCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(ChangeSiteModelCategoryCommand command,
        CancellationToken cancellationToken)
        => await transaction.RunAsync(ct => HandleInTransactionAsync(command, ct), cancellationToken);

    private async Task<Result<SiteModelDto>> HandleInTransactionAsync(ChangeSiteModelCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var id = ProductGroupId.Create(command.ModelId);
        var model = await db.ProductGroups.SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        if (model is null)
            return Result.NotFound();

        var categoryId = ProductCategoryId.Create(command.Request.CategoryId);
        if (!await changes.IsFinalCategoryAsync(categoryId, cancellationToken))
        {
            return SiteModelValidation.Invalid("categoryId",
                "Модель може бути лише в кінцевій категорії (без підкатегорій).", "CategoryNotFinal");
        }

        var now = clock.GetUtcNow();
        if (model.CategoryId != categoryId)
        {
            await changes.AcceptAsync([model], ReviewerName.Of(currentUser), now, cancellationToken);
            model.ChangeCategory(categoryId, now);
            await models.SaveChangesAsync(cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
