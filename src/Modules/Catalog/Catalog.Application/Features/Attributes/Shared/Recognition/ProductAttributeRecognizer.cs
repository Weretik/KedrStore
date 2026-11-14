using Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;
using Catalog.Application.Features.Attributes.Shared.Recognition.ProductNames;

namespace Catalog.Application.Features.Attributes.Shared.Recognition;

/// <summary>
/// The .NET port of the attribute recognizer (<c>docs/catalog-attributes/extract_attributes.py</c>, version 2.0):
/// the same name and dictionary give the same row as <c>products_attributes.csv</c>. It reads names only and never
/// changes them.
/// </summary>
public static class ProductAttributeRecognizer
{
    public const string Version = "2.0";

    public static RecognitionRow Recognize(RecognitionProduct product, RecognitionDictionary dictionary, CategoryPaths categories)
        => new ProductNameRecognition(product, dictionary, categories).Finish();

    /// <summary>
    /// Whether a name may contain the code as 1C writes it, Cyrillic look-alike letters and case included. Used to
    /// pick the products to recognize again after the owner decodes the code; a false positive only costs time.
    /// </summary>
    public static bool Mentions(string name, string rawValue)
        => rawValue.Trim() is { Length: > 0 } code
           && PythonRegex.Search(RecognitionPatterns.Code(code.ToUpperInvariant()), name) is not null;
}
