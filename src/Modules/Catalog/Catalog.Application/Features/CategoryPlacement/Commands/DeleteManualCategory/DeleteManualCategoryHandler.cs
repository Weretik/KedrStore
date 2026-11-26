using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.CategoryPlacement.Shared;

namespace Catalog.Application.Features.CategoryPlacement.Commands.DeleteManualCategory;

/// <summary>
/// Deletes a category made by hand while it has no active subcategories, site models, or products (MC-04); a 1C
/// category answers 400, since the next sync would bring it back.
/// </summary>
public sealed class DeleteManualCategoryHandler(
    ICatalogRepository<ProductCategory> categories,
    IReadCatalogDbContext db,
    TimeProvider clock)
    : ICommandHandler<DeleteManualCategoryCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(DeleteManualCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = ProductCategoryId.Create(command.CategoryId);
        var category = await categories.GetByIdAsync(id, cancellationToken);
        if (category is null || category.IsDeleted)
            return Result.NotFound();
        var manual = category.EnsureManual();
        if (manual.IsFailure)
            return manual.Error!.ToResult<bool>();

        var occupied = await db.Categories.AnyAsync(child => child.ParentId == id, cancellationToken) ||
                       await db.ProductGroups.AnyAsync(model => model.CategoryId == id, cancellationToken) ||
                       await db.Products.AnyAsync(product => product.CategoryId == id, cancellationToken);
        if (occupied)
            return CategoryErrors.NotEmpty().ToResult<bool>();

        if (category.MarkDeleted(clock.GetUtcNow()))
            await categories.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
