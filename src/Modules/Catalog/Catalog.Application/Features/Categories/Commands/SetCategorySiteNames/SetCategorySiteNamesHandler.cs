using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteNames;

/// <summary>
/// Saves the admin's Ukrainian and Russian site names of a category (spec catalog/011 R-02). They win over 1C; a
/// blank one goes back at once to the name 1C and the configuration give, so the site never waits for the next sync.
/// </summary>
public sealed class SetCategorySiteNamesHandler(
    ICatalogRepository<ProductCategory> repository,
    IProductMediaLibrary media,
    IOptions<CategoryPresentationOptions> presentation,
    TimeProvider clock)
    : ICommandHandler<SetCategorySiteNamesCommand, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(SetCategorySiteNamesCommand command,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(ProductCategoryId.Create(command.CategoryId), cancellationToken);
        if (category is null)
            return Result.NotFound();

        var source = new CategoryPresentationResolver(presentation.Value)
            .Resolve(category.ProductTypeIdOneC, category.Id.Value, category.Name, category.Level);
        var changed = category.SetSiteNames(command.NameUk, command.NameRu, source.ShortNameUk, source.ShortNameRu,
            clock.GetUtcNow());
        if (changed.IsFailure)
            return changed.Error!.ToResult<AdminCategoryResponse>();
        if (changed.Value)
            await repository.SaveChangesAsync(cancellationToken);

        var url = await CategoryShareImage.UrlAsync(media, category.ShareImageMediaFileId, cancellationToken);
        return Result.Success(CategoryReadMapper.ToAdminResponse(category, url));
    }
}
