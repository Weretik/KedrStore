using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Attributes;
using Catalog.Contracts.Products.GetAdminList;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetAdminList;

/// <summary>
/// The attribute problems and variant group of admin list rows (spec catalog/006, phase 3): the «Тільки проблемні» and
/// reason filters, and the «Група» and problem columns.
/// </summary>
internal static class AdminProductProblems
{
    public static async Task<IQueryable<ProductListProjection>> FilterAsync(IReadCatalogDbContext db,
        IQueryable<ProductListProjection> products, GetAdminProductsRequest request, CancellationToken cancellationToken)
    {
        var reason = request.ProblemReason?.Trim();
        if (!string.IsNullOrEmpty(reason))
        {
            // Reasons are a jsonb list; the few thousand products in review are filtered in memory.
            var reviews = await db.ProductAttributeReviews.AsNoTracking()
                .Where(review => review.State == AttributeReviewState.NeedsReview)
                .Select(review => new { review.ProductId, review.Reasons })
                .ToListAsync(cancellationToken);
            var ids = reviews.Where(review => review.Reasons.Any(item => item.Code == reason))
                .Select(review => review.ProductId)
                .ToArray();
            return products.Where(projection => ids.Contains(projection.ProductId));
        }

        if (request.ProblemsOnly == true)
        {
            return products.Where(projection => db.ProductAttributeReviews.Any(review =>
                review.ProductId == projection.ProductId && review.State == AttributeReviewState.NeedsReview));
        }

        return products;
    }

    public static async Task<List<AdminProductListRowDto>> EnrichAsync(IReadCatalogDbContext db,
        List<AdminProductListRowDto> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return rows;

        var ids = rows.Select(row => ProductId.From(row.Id)).ToArray();
        var groups = await (from member in db.ProductGroupMembers.AsNoTracking()
                            from productGroup in db.ProductGroups.AsNoTracking()
                            where member.GroupId == productGroup.Id && ids.Contains(member.ProductId)
                            select new { member.ProductId, productGroup.Id, productGroup.Name })
            .ToDictionaryAsync(item => item.ProductId.Value, cancellationToken);
        var problems = await db.ProductAttributeReviews.AsNoTracking()
            .Where(review => ids.Contains(review.ProductId) && review.State == AttributeReviewState.NeedsReview)
            .Select(review => new { review.ProductId, review.Reasons })
            .ToDictionaryAsync(review => review.ProductId.Value, review => review.Reasons, cancellationToken);

        return rows.Select(row => row with
        {
            GroupId = groups.TryGetValue(row.Id, out var group) ? group.Id.Value : null,
            GroupName = group?.Name,
            Problems = problems.TryGetValue(row.Id, out var reasons)
                ? reasons.Select(item => new AdminAttributeProblemDto(item.Code, item.Text)).ToList()
                : []
        }).ToList();
    }
}
