namespace Catalog.Application.Features.SiteModels.Common;

/// <summary>Problem codes of the site models (spec catalog/007 R-08).</summary>
public static class SiteModelProblems
{
    /// <summary>Hard error: an on-site product in no model, proposed or accepted.</summary>
    public const string NoModel = "no_model";
    public const string NoAttributes = "no_attributes";
    public const string MissingAxisValue = "missing_axis_value";
    public const string SameCombination = "same_combination";
    public const string ModelNoAxes = "model_no_axes";
    public const string ModelNoSeo = "model_no_seo";
    public const string ModelNoPhoto = "model_no_photo";

    /// <summary>List filter value: any problem at all.</summary>
    public const string Any = "any";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NoModel, NoAttributes, MissingAxisValue, SameCombination, ModelNoAxes, ModelNoSeo, ModelNoPhoto
    };

    /// <summary>Codes a product can have.</summary>
    public static readonly IReadOnlySet<string> Product = new HashSet<string>(StringComparer.Ordinal)
    {
        NoModel, NoAttributes, MissingAxisValue, SameCombination
    };

    /// <summary>Whether a row with these problems passes the list filter (a code, <see cref="Any"/>, or none).</summary>
    public static bool Passes(IReadOnlyCollection<string> problems, string? filter)
        => filter is null || (filter == Any ? problems.Count > 0 : problems.Contains(filter));
}
