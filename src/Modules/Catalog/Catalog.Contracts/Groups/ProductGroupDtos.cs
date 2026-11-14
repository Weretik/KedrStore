namespace Catalog.Contracts.Groups;

/// <summary>One variant of a group as the admin sees it. Colors are in order: main color first.</summary>
public sealed record AdminProductGroupMemberDto(
    int ProductId,
    string Name,
    string Slug,
    bool ExportToSite,
    bool IsDeleted,
    string Photo,
    decimal Stock,
    bool IsPinned,
    bool IsMain,
    IReadOnlyList<string> Colors,
    string? Size,
    string? Side,
    bool HasSameVariants);

/// <summary>A variant group: products of one model that differ by color, size, or side.</summary>
public sealed record AdminProductGroupDto(
    int Id,
    int CategoryId,
    string Name,
    string Slug,
    int? MainProductId,
    bool IsMainManual,
    bool IsCreatedManually,
    IReadOnlyList<AdminProductGroupMemberDto> Members);

/// <summary>
/// The group of one product. <c>IsPinned</c>: a person set it, automation leaves it; a pinned product without a
/// group was taken out of groups on purpose.
/// </summary>
public sealed record AdminProductGroupMembershipDto(int ProductId, bool IsPinned, AdminProductGroupDto? Group);

public sealed record AdminProductGroupListItemDto(
    int Id,
    int CategoryId,
    string Name,
    string Slug,
    int ProductCount,
    bool IsCreatedManually);

/// <summary>Group search for «Перенести в іншу групу»: by category and part of the name.</summary>
public sealed record GetAdminProductGroupsRequest
{
    public int? CategoryId { get; init; }
    public string? Search { get; init; }
}

/// <summary>A new group a person creates; the listed products move into it and stay there.</summary>
public sealed record CreateProductGroupRequest
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<int> ProductIds { get; init; } = [];
}

public sealed record RenameProductGroupRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed record SetProductGroupMainRequest
{
    public int ProductId { get; init; }
}

/// <summary>
/// Moves a product: <c>Automatic=true</c> returns it to automatic grouping; otherwise the product is pinned to
/// <c>GroupId</c>, or kept out of groups when <c>GroupId</c> is <c>null</c>.
/// </summary>
public sealed record SetProductGroupRequest
{
    public int? GroupId { get; init; }
    public bool Automatic { get; init; }
}
