namespace Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;

public sealed class GetAdminCategoryByIdValidator : AbstractValidator<GetAdminCategoryByIdQuery>
{
    public GetAdminCategoryByIdValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
