namespace Catalog.Application.Features.ProductMedia.Shared;

/// <summary>An active product with its media links, tracked for an edit.</summary>
internal sealed class ProductForMediaSpec : SingleResultSpecification<Product>
{
    public ProductForMediaSpec(ProductId id) => Query.Where(product => product.Id == id);
}

/// <summary>Every product (deleted ones too) that links to the library file, tracked.</summary>
internal sealed class ProductsLinkedToMediaFileSpec : Specification<Product>
{
    public ProductsLinkedToMediaFileSpec(long fileId) => Query
        .IgnoreQueryFilters()
        .Where(product => product.MediaLinks.Any(link => link.MediaFileId == fileId));
}
