using Catalog.Application.Features.Products.Queries.GetAdminProductList;
using Catalog.Application.Features.Products.Queries.GetAllAdminProducts;
using Catalog.Application.Shared;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Products.Shared;

/// <summary>
/// Admin-only page of soft-deleted products (R-005). Deleted products are not in the list projection, so this reads
/// products directly; search matches the Ukrainian name or the 1C code.
/// </summary>
internal static class DeletedAdminProducts
{
    public static async Task<PagedResult<List<AdminProductListRowDto>>> PageAsync(
        IReadCatalogDbContext catalogDbContext,
        GetAdminProductsRequest request,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var products = catalogDbContext.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var pattern = "%" + LikePattern.Escape(term) + "%";
            products = int.TryParse(term, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
                ? products.Where(product => product.Id == ProductId.From(id))
                : products.Where(product => EF.Functions.ILike(product.Name, pattern, @"\"));
        }

        var totalRecords = await products.LongCountAsync(cancellationToken);
        var categories = catalogDbContext.Categories.IgnoreQueryFilters().AsNoTracking();
        var items = await products
            .OrderByDescending(product => product.DeletedAt)
            .ThenBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new AdminProductListRowDto
            {
                Id = product.Id.Value,
                NameUk = product.Name,
                NameRu = product.Name,
                ProductSlug = product.ProductSlug,
                Photo = product.Photo,
                CategoryId = product.CategoryId.Value,
                CategoryName = categories.Where(category => category.Id == product.CategoryId)
                    .Select(category => category.ShortNameUk).FirstOrDefault() ?? string.Empty,
                IsSale = product.IsSale,
                IsNew = product.IsNew,
                ExportToSite = product.ExportToSite,
                Stock = product.Stock,
                QuantityInPack = product.QuantityInPack,
                IsDeleted = true,
                DeletedAt = product.DeletedAt
            })
            .ToListAsync(cancellationToken);

        var pagedInfo = new PagedInfo(pageNumber, pageSize, (long)Math.Ceiling(totalRecords / (double)pageSize), totalRecords);
        return new PagedResult<List<AdminProductListRowDto>>(pagedInfo, items);
    }
}
