using Catalog.Application.Features.Attributes.Jobs;

namespace Catalog.Application.Features.Groups.Shared;

/// <summary>What the group build did (or, without <c>apply</c>, would do).</summary>
public sealed class ProductGroupBuildReport
{
    /// <summary>Skipped: groups a person created; automation only updates their main product.</summary>
    public AttributeImportCounters Groups { get; } = new();
    /// <summary>Skipped: rows a person pinned.</summary>
    public AttributeImportCounters Members { get; } = new();
    public int GroupedProducts { get; set; }
    /// <summary>Groups with at least two products that no switcher tells apart («Однакові варіанти»).</summary>
    public int GroupsWithSameVariants { get; set; }
    public int ProductsWithSameVariants { get; set; }
    public int ReviewsChanged { get; set; }
}
