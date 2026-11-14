namespace Catalog.Application.Features.ProductAttributes.Commands.AcceptProductReview;

public sealed class AcceptProductReviewValidator : AbstractValidator<AcceptProductReviewCommand>
{
    public AcceptProductReviewValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
