using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;

/// <summary>
/// Adds products of the model's category; a product leaves the proposed model it was in, a product of another
/// accepted model is refused (R-04). The products are pinned.
/// </summary>
public sealed class AddSiteModelProductsHandler(
    IReadCatalogDbContext db,
    SiteModelChanges changes,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<AddSiteModelProductsCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(AddSiteModelProductsCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        var categoryId = await db.ProductGroups.Where(group => group.Id == modelId)
            .Select(group => (ProductCategoryId?)group.CategoryId)
            .SingleOrDefaultAsync(cancellationToken);
        if (categoryId is null)
            return Result.NotFound();

        var productIds = command.Request.ProductIds.Select(ProductId.From).ToList();
        if (await changes.CheckProductsAsync(categoryId.Value, productIds, modelId, cancellationToken) is { } error)
            return error;

        await changes.PinAsync(productIds, modelId, clock.GetUtcNow(), cancellationToken);
        await changes.TidyAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}
