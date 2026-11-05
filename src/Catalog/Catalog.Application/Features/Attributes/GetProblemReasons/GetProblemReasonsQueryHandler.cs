using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Enums;

namespace Catalog.Application.Features.Attributes.GetProblemReasons;

public sealed class GetProblemReasonsQueryHandler(IReadCatalogDbContext db)
    : IQueryHandler<GetProblemReasonsQuery, Result<List<AdminProblemReasonDto>>>
{
    public async ValueTask<Result<List<AdminProblemReasonDto>>> Handle(GetProblemReasonsQuery query,
        CancellationToken cancellationToken)
    {
        var onSite = db.Products.AsNoTracking().Where(product => product.ExportToSite).Select(product => product.Id);
        var reasons = await db.ProductAttributeReviews.AsNoTracking()
            .Where(review => review.State == AttributeReviewState.NeedsReview && onSite.Contains(review.ProductId))
            .Select(review => review.Reasons)
            .ToListAsync(cancellationToken);
        return Result.Success(reasons
            .SelectMany(list => list.Select(reason => reason.Code).Distinct())
            .GroupBy(code => code)
            .Select(group => new AdminProblemReasonDto(group.Key, group.Count()))
            .OrderByDescending(item => item.ProductCount).ThenBy(item => item.Code, StringComparer.Ordinal)
            .ToList());
    }
}
