namespace Application.Catalog.Queries.GetCategories;

public class GetCategoriesQuryHandler(
    ICatalogReadRepository<Category> categoryRepository)
    : IQueryHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    public async ValueTask<Result<List<CategoryDto>>> Handle(
        GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var spec = new AllCategoriesDtoSpecification();
        var flat = await categoryRepository.ListAsync(spec, cancellationToken);

        if (flat.Count == 0)
            return Result.NotFound();

        var lookup = flat.ToLookup(c => c.ParentCategoryId);
        var tree = BuildTree(lookup,null);

        return Result.Success(tree);
    }

    private static List<CategoryDto> BuildTree(
        ILookup<int?, CategoryDto> lookup, int? parentId)
    {
        return lookup[parentId]
            .Select(c => new CategoryDto
            {
                Id               = c.Id,
                Name             = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                Children         = BuildTree(lookup, c.Id)
            })
            .ToList();
    }
}
