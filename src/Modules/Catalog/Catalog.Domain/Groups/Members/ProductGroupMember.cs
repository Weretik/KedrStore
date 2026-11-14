using Catalog.Domain.Products;
namespace Catalog.Domain.Groups.Members;

/// <summary>
/// The group of one product (a product has at most one). A pinned row was set by a person: automation never moves
/// it again. A pinned row without a group keeps the product out of automatic groups.
/// </summary>
public class ProductGroupMember : IAggregateRoot
{
    public ProductId ProductId { get; private set; }
    public ProductGroupId? GroupId { get; private set; }
    public bool IsPinned { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ProductGroupMember() { }

    public static ProductGroupMember CreateAutomatic(ProductId productId, ProductGroupId groupId, DateTimeOffset now)
        => new() { ProductId = productId, GroupId = groupId, UpdatedAt = now };

    public static ProductGroupMember CreatePinned(ProductId productId, ProductGroupId? groupId, DateTimeOffset now)
        => new() { ProductId = productId, GroupId = groupId, IsPinned = true, UpdatedAt = now };

    /// <summary>Moves an automatic row; a pinned row is left as it is. Returns whether it changed.</summary>
    public bool MoveAutomatically(ProductGroupId groupId, DateTimeOffset now)
    {
        if (IsPinned || GroupId == groupId)
            return false;

        GroupId = groupId;
        UpdatedAt = now;
        return true;
    }

    /// <summary>A person puts the product into a group, or keeps it out of groups with <c>null</c>.</summary>
    public void Pin(ProductGroupId? groupId, DateTimeOffset now)
    {
        GroupId = groupId;
        IsPinned = true;
        UpdatedAt = now;
    }

    /// <summary>A person returns the product to automatic grouping; the next build decides its group.</summary>
    public void Unpin(DateTimeOffset now)
    {
        IsPinned = false;
        UpdatedAt = now;
    }
}
