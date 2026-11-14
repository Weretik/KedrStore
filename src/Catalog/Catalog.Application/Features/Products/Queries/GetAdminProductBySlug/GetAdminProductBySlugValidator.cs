namespace Catalog.Application.Features.Products.Queries.GetAdminProductBySlug;

public sealed class GetAdminProductBySlugValidator : AbstractValidator<GetAdminProductBySlugQuery>
{
    public GetAdminProductBySlugValidator()
    {
        RuleFor(query => query.ProductSlug)
            .NotEmpty()
            .MaximumLength(255);
    }
}
