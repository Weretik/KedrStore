using Catalog.Application.Attributes.Import;
using Catalog.Application.Attributes.Recognition;

namespace UnitTests.Catalog.Attributes;

/// <summary>
/// Spec catalog/009: a code may mean values of other attributes too. They reach the products whose names contain the
/// code as the recognizer found it, win over recognized values of the same attribute, and a category with its own
/// decoding uses only its own meanings.
/// </summary>
public sealed class CodeMeaningsTests
{
    private const int Cylinders = 2680;
    private static readonly (string, string, string) Zcg = ("cylinder_configuration_code", "mechanical_cylinder", "zcg");

    private static readonly RecognizedProduct Product = new(7894, 7894, "KEDR циліндр BRASS",
        [new RecognizedValue("color", 0, "SN", null, false), new RecognizedValue("side", 0, "L", null, true)], []);

    private static CodeMeaningSet Set(IReadOnlyList<(string, string)> forAll,
        Dictionary<int, IReadOnlyList<(string, string)>>? byCategory = null)
        => new(forAll, byCategory ?? []);

    [Fact]
    public void FoundCode_AddsItsMeanings_AndReplacesRecognizedValuesOfThatAttribute()
    {
        var result = CodeMeanings.Apply(Product, Cylinders, [Zcg],
            new Dictionary<(string, string, string), CodeMeaningSet> { [Zcg] = Set([("color", "G"), ("configuration", "Z")]) });

        Assert.Equal(
            [("side", 0, "L", true), ("color", 0, "G", true), ("configuration", 0, "Z", true)],
            result.Values.Select(value => (value.AttributeCode, value.Position, value.ValueCode, value.IsConfirmed)));
    }

    [Fact]
    public void CodeNotFoundInTheName_ChangesNothing()
    {
        var result = CodeMeanings.Apply(Product, Cylinders, [],
            new Dictionary<(string, string, string), CodeMeaningSet> { [Zcg] = Set([("color", "G")]) });

        Assert.Same(Product, result);
    }

    [Fact]
    public void CategoryWithItsOwnDecoding_UsesOnlyItsOwnMeanings()
    {
        var meanings = new Dictionary<(string, string, string), CodeMeaningSet>
        {
            [Zcg] = Set([("color", "G")], new() { [Cylinders] = [] })
        };

        Assert.Same(Product, CodeMeanings.Apply(Product, Cylinders, [Zcg], meanings));
        Assert.Contains(CodeMeanings.Apply(Product, 1, [Zcg], meanings).Values, value => value.ValueCode == "G");
    }

    [Fact]
    public void Recognizer_ReportsTheCodesItFound()
    {
        var categories = CategoryPaths.FromPaths(new Dictionary<int, string> { [Cylinders] = "Фурнітура > Циліндри > серія BRASS" });
        using var file = RecognitionDictionaryFile.Open();
        var row = ProductAttributeRecognizer.Recognize(
            new RecognitionProduct(7894, Cylinders, "KEDR циліндр BRASS M60 ZCG (30T*30) brass key"),
            RecognitionDictionary.Read(file, categories), categories);

        Assert.Contains(Zcg, row.FoundCodes);
    }
}
