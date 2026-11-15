using BuildingBlocks.Application.Text;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroups;

public sealed class GetProductGroupsHandler(IReadCatalogDbContext db)
    : IQueryHandler<GetProductGroupsQuery, Result<List<AdminProductGroupListItemDto>>>
{
    public async ValueTask<Result<List<AdminProductGroupListItemDto>>> Handle(GetProductGroupsQuery query,
        CancellationToken cancellationToken)
    {
        var groups = db.ProductGroups.AsNoTracking();
        if (query.Request.CategoryId is { } categoryId)
            groups = groups.Where(group => group.CategoryId == ProductCategoryId.From(categoryId));

        // Database rules 2.1: every word must match; a trigram index serves ILIKE on the name.
        foreach (var pattern in SearchWords.Patterns(query.Request.Search))
            groups = groups.Where(group => EF.Functions.ILike(group.Name, pattern, SearchWords.EscapeCharacter));

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
}
