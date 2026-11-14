using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Shared;

/// <summary>Pins products by a person's choice; automation never moves a pinned product again.</summary>
public sealed class ProductGroupPins(IReadCatalogDbContext db, ICatalogRepository<ProductGroupMember> members)
{
    /// <summary>Puts each product into <paramref name="groupId"/> (or keeps it out of groups with <c>null</c>). Saves.</summary>
    public async Task PinAsync(IReadOnlyCollection<ProductId> productIds, ProductGroupId? groupId, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.ProductGroupMembers
            .Where(member => productIds.Contains(member.ProductId))
            .ToDictionaryAsync(member => member.ProductId, cancellationToken);

        var created = new List<ProductGroupMember>();
        foreach (var productId in productIds)
        {
            if (rows.TryGetValue(productId, out var row))
                row.Pin(groupId, now);
            else
                created.Add(ProductGroupMember.CreatePinned(productId, groupId, now));
        }

        if (created.Count > 0)
            await members.AddRangeAsync(created, cancellationToken);
        await members.SaveChangesAsync(cancellationToken);
    }
}
