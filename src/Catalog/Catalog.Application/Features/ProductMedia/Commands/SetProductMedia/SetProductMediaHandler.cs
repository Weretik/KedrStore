using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.ProductMedia.Shared;
using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Commands.SetProductMedia;

/// <summary>
/// Replaces the media set of a product (R-31). Every ID must exist in the library; the cached photo and scheme and
/// the list row follow the links.
/// </summary>
public sealed class SetProductMediaHandler(
    ICatalogRepository<Product> products,
    IProductMediaLibrary library,
    IProductListProjectionRebuilder projections,
    TimeProvider clock)
    : ICommandHandler<SetProductMediaCommand, Result<ProductMediaDto>>
{
    public async ValueTask<Result<ProductMediaDto>> Handle(SetProductMediaCommand command,
        CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(
            new ProductForMediaSpec(ProductId.From(command.ProductId)), cancellationToken);
        if (product is null)
            return Result.NotFound();

        var request = command.Request;
        var wantedIds = request.PhotoIds.Concat(request.SchemeId is { } schemeId ? [schemeId] : []).ToArray();
        var files = (await library.GetByIdsAsync(wantedIds, cancellationToken)).ToDictionary(file => file.Id);
        var unknown = wantedIds.Where(id => !files.ContainsKey(id)).ToArray();
        if (unknown.Length > 0)
        {
            return Result.Invalid(new ValidationError(
                "mediaFileIds", $"Цих файлів немає в медіатеці: {string.Join(", ", unknown)}.",
                "Catalog.Product.Media.FileNotFound", ValidationSeverity.Error));
        }

        ProductMediaFile ToFile(long id) => new(id, files[id].Url);
        var changed = product.SetMedia(
            request.PhotoIds.Select(ToFile).ToArray(),
            request.SchemeId is { } scheme ? ToFile(scheme) : null,
            clock.GetUtcNow());
        if (changed.IsFailure)
            return changed.Error!.ToResult<ProductMediaDto>();

        if (changed.Value)
        {
            await products.SaveChangesAsync(cancellationToken);
            await projections.RefreshPhotosAsync(cancellationToken);
        }

        return Result.Success(ProductMediaMapper.ToDto(product, files.Values));
    }
}
