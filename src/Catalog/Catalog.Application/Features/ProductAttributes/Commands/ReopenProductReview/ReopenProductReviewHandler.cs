using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;

public sealed class ReopenProductReviewHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductAttributeReview> reviews,
    ProductAttributesReader reader,
    TimeProvider clock)
    : ICommandHandler<ReopenProductReviewCommand, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(ReopenProductReviewCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.ProductId);
        var review = await db.ProductAttributeReviews
            .FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
        if (review is null)
        {
            return await db.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == productId, cancellationToken)
                ? AttributeValidation.Invalid("productId", "Для товару ще не розпізнано властивостей.", "NoReview")
                : Result.NotFound();
        }

        if (review.State == AttributeReviewState.Accepted)
        {
            review.Reopen(clock.GetUtcNow());
            await reviews.SaveChangesAsync(cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(productId, cancellationToken))!);
    }
}
