namespace Catalog.Application.Features.Products.GetAdminDetail;

public sealed class GetAdminProductBySlugQueryValidator : AbstractValidator<GetAdminProductBySlugQuery>
{
    public GetAdminProductBySlugQueryValidator()
    {
        RuleFor(query => query.ProductSlug)
            .NotEmpty()
            .MaximumLength(255);
    }
}
