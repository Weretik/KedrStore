namespace Catalog.Application.Features.Category.AdminGetById;

public sealed class GetAdminCategoryByIdQueryValidator : AbstractValidator<GetAdminCategoryByIdQuery>
{
    public GetAdminCategoryByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
