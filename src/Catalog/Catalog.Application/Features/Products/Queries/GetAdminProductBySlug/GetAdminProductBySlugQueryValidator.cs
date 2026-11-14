namespace Catalog.Application.Features.Products.Queries.GetAdminProductBySlug;

public sealed class GetAdminProductBySlugQueryValidator : AbstractValidator<GetAdminProductBySlugQuery>
{
    public GetAdminProductBySlugQueryValidator()
    {
        RuleFor(query => query.ProductSlug)
            .NotEmpty()
            .MaximumLength(255);
    }
}
