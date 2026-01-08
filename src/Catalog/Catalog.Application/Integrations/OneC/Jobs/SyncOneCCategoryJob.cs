using BuildingBlocks.Application.Integrations.OneC.Contracts;
using BuildingBlocks.Application.Integrations.OneC.RootCategoryId;
using Catalog.Application.Features.Shared;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Persistance;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCCategoryJob(IOneCClient oneC, ICatalogRepository<ProductCategory> categoryRepo, IOptionsSnapshot<OneCOptions> options)
{
    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        var rootCategoryId = int.Parse(rootCategoryOneCId.TrimStart('0'));
        var furnitureId = int.Parse(options.Value.HardwareRootCategoryId.TrimStart('0'));

        var categoriesOneC = await oneC.GetCategoriesAsync(rootCategoryOneCId, cancellationToken);

        if (categoriesOneC.Count == 0)
            return;

        var categoriesOneIds = categoriesOneC
            .Where(x => !string.IsNullOrWhiteSpace(x.CategoryId))
            .Select(x => ProductCategoryId.From(int.Parse(x.CategoryId.TrimStart('0')))).ToList();

        if (categoriesOneIds.Count == 0)
            return;

        var categories = CatalogMapper.MapCategory(categoriesOneC, rootCategoryId, furnitureId);

        await DeleteMissingAsync(categories, cancellationToken);
        await CreateOrUpsertCategoriesAsync(categories, cancellationToken);
    }

    private async Task DeleteMissingAsync(IReadOnlyList<CategoryDto> categoryDtos, CancellationToken cancellationToken)
    {
        var importCategoryIds = categoryDtos
            .Select(c => ProductCategoryId.From(c.Id))
            .Distinct()
            .ToArray();

        var categorySpec  = new CategoriesByIdsSpec(importCategoryIds, true);
        await categoryRepo.DeleteRangeAsync(categorySpec, cancellationToken);
    }

    private async Task CreateOrUpsertCategoriesAsync(IReadOnlyList<CategoryDto> categoryDtos, CancellationToken cancellationToken)
    {

        foreach (var item in categoryDtos)
        {
            var id = ProductCategoryId.From(item.Id);
            ProductCategoryId? parentId = item.ParentId == null ? null : ProductCategoryId.From(item.Id);
            CategoryPath path = CategoryPath.From(item.Path);
            var existing = await categoryRepo.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var productCategory = ProductCategory.Create(
                    id: id,
                    name: item.Name,
                    slug: item.Slug,
                    path: path,
                    parentId: parentId
                );
                await categoryRepo.AddAsync(productCategory, cancellationToken);
            }
            else
            {
                existing.Update(
                    name: item.Name,
                    slug: item.Slug,
                    path: path,
                    parentId: parentId);
            }
        }
        await categoryRepo.SaveChangesAsync(cancellationToken);
    }
}
