namespace Catalog.Application.Features.ProductAttributes.Commands.AcceptProductReview;

public sealed class AcceptProductReviewCommandValidator : AbstractValidator<AcceptProductReviewCommand>
{
    public AcceptProductReviewCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
