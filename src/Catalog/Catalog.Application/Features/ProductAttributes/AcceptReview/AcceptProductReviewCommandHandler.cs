using BuildingBlocks.Application.Contracts;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Application.Features.ProductAttributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.ProductAttributes.AcceptReview;

public sealed class AcceptProductReviewCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductAttributeReview> reviews,
    ProductAttributesReader reader,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<AcceptProductReviewCommand, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(AcceptProductReviewCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.ProductId);
        var review = await db.ProductAttributeReviews
            .FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
        if (review is null)
        {
            return await db.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == productId, cancellationToken)
                ? AttributeValidation.Invalid("productId", "The product has no recognized attributes yet.", "NoReview")
                : Result.NotFound();
        }

        if (review.State != AttributeReviewState.Accepted)
        {
            review.Accept(ReviewerName.Of(currentUser), clock.GetUtcNow());
            await reviews.SaveChangesAsync(cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(productId, cancellationToken))!);
    }
}
