using Catalog.Application.Features.Groups.Shared;

namespace Catalog.Application.Features.Attributes.Jobs;

/// <summary>Counters of one stage of the attribute import.</summary>
public sealed class AttributeImportCounters
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    /// <summary>Rows a person set or locked; the import leaves them as they are.</summary>
    public int Skipped { get; set; }
    /// <summary>Automatic product values the new file no longer has.</summary>
    public int Removed { get; set; }

    public override string ToString()
        => $"created={Created}; updated={Updated}; unchanged={Unchanged}; skipped={Skipped}; removed={Removed}";
}

/// <summary>What the import did (or, without <c>--apply</c>, would do).</summary>
public sealed class AttributeImportReport
{
    public const int MaxErrorsShown = 20;

    public bool Applied { get; init; }
    public AttributeImportCounters Attributes { get; } = new();
    public AttributeImportCounters Values { get; } = new();
    public AttributeImportCounters Aliases { get; } = new();
    public AttributeImportCounters ProductValues { get; } = new();
    public AttributeImportCounters Reviews { get; } = new();
    public AttributeImportCounters CategoryAttributes { get; } = new();
    public ProductGroupBuildReport Groups { get; set; } = new();
    public int ProductsInFile { get; set; }
    public int ProductsNeedingReview { get; set; }
    public List<string> Errors { get; } = [];
}
