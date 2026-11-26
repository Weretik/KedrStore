using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySearchSettings;

/// <summary>
/// Closes or opens a category for search engines with every subcategory (<see cref="CategorySearchClosing"/>) and sets
/// its link-preview picture (spec catalog/010 R-01, R-03). A newly chosen picture must be in the media library.
/// </summary>
public sealed class SetCategorySearchSettingsHandler(
    ICatalogRepository<ProductCategory> repository,
    IProductMediaLibrary media,
    TimeProvider clock)
    : ICommandHandler<SetCategorySearchSettingsCommand, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(SetCategorySearchSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesForSearchSpec(), cancellationToken);
        var selected = categories.FirstOrDefault(category => category.Id.Value == command.CategoryId && !category.IsDeleted);
        if (selected is null)
            return Result.NotFound();

        var imageId = command.ShareImageMediaFileId is { } id ? MediaFileId.Create(id) : (MediaFileId?)null;
        var url = await CategoryShareImage.UrlAsync(media, imageId, cancellationToken);
        if (imageId is not null && url is null && imageId != selected.ShareImageMediaFileId)
        {
            return Result.Invalid(new ValidationError("shareImageMediaFileId", "Цього зображення немає в медіатеці.",
                "Catalog.Category.ShareImage.NotFound", ValidationSeverity.Error));
        }

        var now = clock.GetUtcNow();
        var image = selected.SetShareImage(imageId, now);
        if (image.IsFailure)
            return image.Error!.ToResult<AdminCategoryResponse>();
        var closed = CategorySearchClosing.Apply(selected, categories, command.IsClosedFromSearch, now);
        if (image.Value || closed > 0)
            await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(CategoryReadMapper.ToAdminResponse(selected, url));
    }
}

/// <summary>Every category, deleted ones included, tracked for the change.</summary>
internal sealed class AllCategoriesForSearchSpec : Specification<ProductCategory>
{
    public AllCategoriesForSearchSpec() => Query.IgnoreQueryFilters();
}
