using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCCategoryJob(IOneCClient oneC, ICatalogRepository<ProductCategory> categoryRepo,
    IOptionsSnapshot<RootCategoryId> options, ILogger<SyncOneCCategoryJob> logger)
{
    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCCategoryJob started for {Root}", rootCategoryOneCId);

        var rootCategoryId = int.Parse(rootCategoryOneCId.TrimStart('0'));
        var furnitureId = int.Parse(options.Value.HardwareRootCategoryId.TrimStart('0'));

        var categoriesOneC = await oneC.GetCategoriesAsync(rootCategoryOneCId, cancellationToken);

        if (categoriesOneC.Count == 0)
            return;

        var categories = CatalogMapper.MapCategory(categoriesOneC, rootCategoryId, furnitureId, rootCategoryOneCId);

        await DeleteMissingAsync(categories, rootCategoryOneCId, cancellationToken);
        await CreateOrUpsertCategoriesAsync(categories, rootCategoryOneCId, cancellationToken);

        logger.LogInformation("SyncOneCCategoryJob finished for {Root}", rootCategoryOneCId);
    }

    private async Task DeleteMissingAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        CancellationToken cancellationToken)
    {
        var importCategoryIds = categoryDtos
            .Select(c => ProductCategoryId.From(c.Id))
            .Distinct()
            .ToArray();

        var categorySpec  = new CategoriesByIdsSpec(importCategoryIds, rootCategoryOneCId,true);
        await categoryRepo.DeleteRangeAsync(categorySpec, cancellationToken);
    }

    private async Task CreateOrUpsertCategoriesAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        CancellationToken cancellationToken)
    {

        foreach (var item in categoryDtos)
        {
            var id = ProductCategoryId.From(item.Id);
            ProductCategoryId? parentId = item.ParentId == null ? null : ProductCategoryId.From(item.ParentId.Value);
            CategoryPath path = CategoryPath.From(item.Path);
            var existing = await categoryRepo.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var productCategory = ProductCategory.Create(
                    id: id,
                    productTypeIdOneC: rootCategoryOneCId,
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
