using Catalog.Application.Features.Products.Shared;

namespace Catalog.Application.Features.Products.Queries.GetPublicProductList;

public sealed class GetPublicProductListValidator : AbstractValidator<GetPublicProductListQuery>
{
    public GetPublicProductListValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит не може бути порожнім.")
            .SetValidator(new GetProductsRequestValidator());
    }
}
