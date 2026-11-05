namespace Catalog.Application.Features.SiteModels.Common;

/// <summary>Problem codes of the site models (spec catalog/007 R-08).</summary>
public static class SiteModelProblems
{
    /// <summary>Hard error: an on-site product outside accepted models.</summary>
    public const string NoModel = "no_model";
    public const string NoAttributes = "no_attributes";
    public const string MissingAxisValue = "missing_axis_value";
    public const string SameCombination = "same_combination";
    public const string ModelNoAxes = "model_no_axes";
    public const string ModelNoSeo = "model_no_seo";
    public const string ModelNoPhoto = "model_no_photo";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NoModel, NoAttributes, MissingAxisValue, SameCombination, ModelNoAxes, ModelNoSeo, ModelNoPhoto
    };
}
