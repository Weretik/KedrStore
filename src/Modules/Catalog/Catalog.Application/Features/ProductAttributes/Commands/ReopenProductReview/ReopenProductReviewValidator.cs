namespace Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;

public sealed class ReopenProductReviewValidator : AbstractValidator<ReopenProductReviewCommand>
{
    public ReopenProductReviewValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
