namespace Catalog.Domain.Groups;

/// <summary>State of a site model (a product group, spec catalog/007).</summary>
public enum ProductGroupStatus : short
{
    /// <summary>Automation proposed it; a person has not accepted it yet. The build may still change it.</summary>
    Proposed = 0,

    /// <summary>A person accepted or created it. Automation never changes its products, name, axes, or texts.</summary>
    Accepted = 1
}
