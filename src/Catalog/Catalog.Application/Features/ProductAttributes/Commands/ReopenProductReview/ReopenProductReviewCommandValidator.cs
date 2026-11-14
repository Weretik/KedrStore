namespace Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;

public sealed class ReopenProductReviewCommandValidator : AbstractValidator<ReopenProductReviewCommand>
{
    public ReopenProductReviewCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
