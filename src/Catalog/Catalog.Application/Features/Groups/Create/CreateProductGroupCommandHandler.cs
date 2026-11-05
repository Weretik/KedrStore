using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Groups.Common;
using Catalog.Application.Groups;
using Catalog.Application.Slugs;
using Catalog.Contracts.Groups;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.Create;

public sealed class CreateProductGroupCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> groups,
    ISlugService slugs,
    ProductGroupPins pins,
    ProductGroupBuilder builder,
    ProductGroupReader reader,
    TimeProvider clock)
    : ICommandHandler<CreateProductGroupCommand, Result<AdminProductGroupDto>>
{
    public async ValueTask<Result<AdminProductGroupDto>> Handle(CreateProductGroupCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var categoryId = ProductCategoryId.From(request.CategoryId);
        if (!await db.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken))
            return ProductGroupValidation.Invalid("categoryId", "Category not found.", "CategoryNotFound");

        var productIds = request.ProductIds.Select(ProductId.From).ToList();
        var inCategory = await db.Products
            .Where(product => productIds.Contains(product.Id) && product.CategoryId == categoryId)
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);
        var wrong = productIds.Except(inCategory).Select(id => id.Value).ToList();
        if (wrong.Count > 0)
        {
            return ProductGroupValidation.Invalid("productIds",
                $"Products not found in this category: {string.Join(", ", wrong)}.", "ProductNotInCategory");
        }

        var now = clock.GetUtcNow();
        var allocator = await slugs.LoadAllocatorAsync(CatalogEntityType.ProductGroup, cancellationToken);
        var group = ProductGroup.CreateManual(categoryId, request.Name, allocator.Allocate(request.Name), now);
        await groups.AddAsync(group, cancellationToken);

        if (productIds.Count > 0)
            await pins.PinAsync(productIds, group.Id, now, cancellationToken);
        await builder.RunAsync(apply: true, cancellationToken);

        return Result.Success((await reader.ReadAsync(group.Id, cancellationToken))!);
    }
}
