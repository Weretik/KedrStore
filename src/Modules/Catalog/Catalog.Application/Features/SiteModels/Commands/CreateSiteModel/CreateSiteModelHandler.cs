using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;

/// <summary>A person creates a model in a final category; it is accepted at once and its products pinned (R-04).</summary>
public sealed class CreateSiteModelHandler(
    ICatalogRepository<ProductGroup> models,
    ISlugService slugs,
    SiteModelChanges changes,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<CreateSiteModelCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(CreateSiteModelCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var categoryId = ProductCategoryId.From(request.CategoryId);
        if (!await changes.IsFinalCategoryAsync(categoryId, cancellationToken))
        {
            return SiteModelValidation.Invalid("categoryId",
                "Модель може бути лише в кінцевій категорії (без підкатегорій).", "CategoryNotFinal");
        }

        var productIds = request.ProductIds.Select(ProductId.From).ToList();
        if (await changes.CheckProductsAsync(categoryId, productIds, null, cancellationToken) is { } error)
            return error;

        var now = clock.GetUtcNow();
        var allocator = await slugs.LoadAllocatorAsync(CatalogEntityType.ProductGroup, cancellationToken);
        var created = ProductGroup.CreateManual(categoryId, request.Name, allocator.Allocate(request.Name), now);
        if (created.IsFailure)
            return created.Error!.ToResult<SiteModelDto>();
        var model = created.Value;
        await models.AddAsync(model, cancellationToken);

        if (productIds.Count > 0)
            await changes.PinAsync(productIds, model.Id, now, cancellationToken);
        await changes.TidyAsync(cancellationToken);
        await changes.SaveOfferedAxesAsync([model.Id], cancellationToken);

        return Result.Success((await reader.ReadAsync(model.Id.Value, cancellationToken))!);
    }
}
