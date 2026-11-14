using BuildingBlocks.Application.Users;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Shared;
using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.AcceptProductReview;

public sealed class AcceptProductReviewHandler(
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
                ? AttributeValidation.Invalid("productId", "Для товару ще не розпізнано властивостей.", "NoReview")
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
