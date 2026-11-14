using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;

public sealed class GetAdminCategoryByIdQueryValidator : AbstractValidator<GetAdminCategoryByIdQuery>
{
    public GetAdminCategoryByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
