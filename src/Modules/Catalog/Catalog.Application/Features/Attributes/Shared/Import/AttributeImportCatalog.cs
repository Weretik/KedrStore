using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Shared.Import;

/// <summary>
/// Built-in attribute settings that the dictionary file does not carry: kind, unit, URL slug, whether the site shows
/// it as a filter, and whether it forms variants (owner decision 2026-11-05: color, size, side).
/// </summary>
public static class AttributeImportCatalog
{
    public const string Color = "color";
    public const string Size = "size";
    public const string Side = "side";
    public const string Branding = "branding";
    public const string NoNameCode = "NO_NAME";

    public static readonly IReadOnlyList<string> SizeNumberAttributes =
        ["size_length", "size_width", "size_thickness", "size_diameter"];

    /// <summary>Attributes shown as filters on the site by default. Admin can change this per category later.</summary>
    private static readonly HashSet<string> Filterable = new(StringComparer.Ordinal)
    {
        Color, Size, Side, "product_type", "material", "shape", "lock_type", "latch_type", "key_type",
        "opening_method", "door_infill", "covering", "construction", "telescopic", "seal_profile", "profile_shape",
        "surface_texture", "guide_type", "key_configuration"
    };

    private static readonly HashSet<string> VariantAxes = new(StringComparer.Ordinal) { Color, Size, Side };

    /// <summary>Short names and URL slugs for the attributes people filter by most.</summary>
    private static readonly Dictionary<string, (string Name, string Slug)> Overrides = new(StringComparer.Ordinal)
    {
        [Color] = ("Колір", "kolir"),
        [Size] = ("Розмір", "rozmir"),
        [Side] = ("Сторона", "storona"),
        ["size_length"] = ("Довжина", "dovzhyna"),
        ["size_width"] = ("Ширина", "shyryna"),
        ["size_thickness"] = ("Товщина", "tovshchyna"),
        ["size_diameter"] = ("Діаметр", "diametr")
    };

    private static readonly List<string> LeadingOrder = [Color, Size, Side, "product_type", "material"];

    public static AttributeKind KindOf(string code)
        => code == Size ? AttributeKind.Size
            : SizeNumberAttributes.Contains(code) ? AttributeKind.Number
            : AttributeKind.Enum;

    public static string? UnitOf(string code) => SizeNumberAttributes.Contains(code) ? "mm" : null;

    public static bool IsFilterable(string code) => Filterable.Contains(code);

    public static bool IsVariantAxis(string code) => VariantAxes.Contains(code);

    public static string NameOf(string code, string dictionaryName)
        => Overrides.TryGetValue(code, out var known) ? known.Name : dictionaryName;

    public static string? SlugOf(string code) => Overrides.TryGetValue(code, out var known) ? known.Slug : null;

    /// <summary>Leading attributes first, then the dictionary order.</summary>
    public static int SortOrderOf(string code, int dictionaryIndex)
    {
        var leading = LeadingOrder.IndexOf(code);
        return leading >= 0 ? leading : LeadingOrder.Count + dictionaryIndex;
    }
}
