namespace Catalog.Application.Features.Products.GetMissingPhotos;

public sealed class GetMissingProductPhotosQueryValidator : AbstractValidator<GetMissingProductPhotosQuery>
{
    public GetMissingProductPhotosQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Request)
            .NotNull()
            .WithMessage("Request cannot be empty.");

        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be >= 1.");

            RuleFor(query => query.Request.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        });
    }
}
