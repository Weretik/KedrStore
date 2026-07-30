using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.GetList.Specifications;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.AdminGetById;

public sealed class GetAdminCategoryByIdQueryHandler(ICatalogReadRepository<ProductCategory> repository)
    : IQueryHandler<GetAdminCategoryByIdQuery, Result<AdminCategoryResponse>>
{
    public async ValueTask<Result<AdminCategoryResponse>> Handle(GetAdminCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var category = categories.FirstOrDefault(x => x.Id.Value == query.Id);
        return category is null ? Result.NotFound() : Result.Success(CategoryReadMapper.ToAdminResponse(category));
    }
}
