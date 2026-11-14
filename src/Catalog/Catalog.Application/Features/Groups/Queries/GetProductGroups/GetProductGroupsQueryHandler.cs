using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroups;

public sealed class GetProductGroupsQueryHandler(IReadCatalogDbContext db)
    : IQueryHandler<GetProductGroupsQuery, Result<List<AdminProductGroupListItemDto>>>
{
    public async ValueTask<Result<List<AdminProductGroupListItemDto>>> Handle(GetProductGroupsQuery query,
        CancellationToken cancellationToken)
    {
        var groups = db.ProductGroups.AsNoTracking();
        if (query.Request.CategoryId is { } categoryId)
            groups = groups.Where(group => group.CategoryId == ProductCategoryId.From(categoryId));

        var search = query.Request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            groups = groups.Where(group => EF.Functions.ILike(group.Name, $"%{EscapeLike(search)}%", @"\"));

        var rows = await groups
            .OrderBy(group => group.Name)
            .Take(GetProductGroupsQuery.MaxResults)
            .Select(group => new
            {
                group.Id, group.CategoryId, group.Name, group.Slug, group.IsCreatedManually,
                Count = db.ProductGroupMembers.Count(member => member.GroupId == group.Id)
            })
            .ToListAsync(cancellationToken);

        return Result.Success(rows.Select(row => new AdminProductGroupListItemDto(row.Id.Value, row.CategoryId.Value,
            row.Name, row.Slug, row.Count, row.IsCreatedManually)).ToList());
    }

    private static string EscapeLike(string text)
        => text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
