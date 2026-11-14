using Catalog.Application.Features.Attributes.Shared.Recognition;
using Catalog.Application.Features.Attributes.Shared.Recognition.Dictionary;

namespace UnitTests.Catalog.Attributes;

/// <summary>
/// Catalog attributes 006, phase 4: the owner's code decisions from «Не розшифровані коди» take priority over the
/// dictionary file, and the products that may contain a code are found by name.
/// </summary>
public sealed class AttributeRecognizerDecisionTests
{
    private const int HandleCategory = 5915;
    private static readonly RecognitionProduct Handle = new(60, HandleCategory, "KEDR  ручка на кв. розетці R08.148-AL-SN/CP");

    private static readonly CategoryPaths Categories = CategoryPaths.FromPaths(new Dictionary<int, string>
    {
        [HandleCategory] = "Фурнітура > Ручки > На планці"
    });

    private static RecognitionRow Recognize(params AttributeCodeDecision[] decisions)
    {
        using var file = RecognitionDictionaryFile.Open();
        return ProductAttributeRecognizer.Recognize(Handle, RecognitionDictionary.Read(file, Categories, decisions),
            Categories);
    }

    [Fact]
    public void WithoutADecision_TheUnresolvedCodeNeedsReview()
    {
        var row = Recognize();

        Assert.Equal("true", row.Cell("needs_review"));
        Assert.Contains("unresolved_material_code", row.Cell("extra_attributes"));
    }

    [Fact]
    public void CodeDecidedAsMeaningNothing_IsIgnored()
    {
        var row = Recognize(new AttributeCodeDecision("material_code", "general", "AL", null, null));

        Assert.DoesNotContain("unresolved_material_code", row.Cell("extra_attributes"));
        Assert.DoesNotContain("«AL»", row.Cell("review_reason") ?? "");
    }

    [Fact]
    public void CodeDecidedAsAValue_GivesThatValue()
    {
        var row = Recognize(new AttributeCodeDecision("material_code", "general", "AL", "ALUMINIUM", "Алюміній"));

        Assert.DoesNotContain("unresolved_material_code", row.Cell("extra_attributes"));
        Assert.Contains("\"ALUMINIUM\"", row.Cell("extra_attributes"));
    }

    [Fact]
    public void CodeDecidedForTheProductCategory_WinsOverTheDecisionForAll()
    {
        var row = Recognize(
            new AttributeCodeDecision("material_code", "general", "AL", "ALUMINIUM", "Алюміній"),
            new AttributeCodeDecision("material_code", "general", "AL", null, null, HandleCategory));

        Assert.DoesNotContain("ALUMINIUM", row.Cell("extra_attributes"));
        Assert.DoesNotContain("unresolved_material_code", row.Cell("extra_attributes"));
    }

    [Fact]
    public void CodeDecidedForAnotherCategory_LeavesTheProductUndecided()
    {
        var row = Recognize(new AttributeCodeDecision("material_code", "general", "AL", "ALUMINIUM", "Алюміній", 1));

        Assert.Equal("true", row.Cell("needs_review"));
        Assert.Contains("unresolved_material_code", row.Cell("extra_attributes"));
    }

    [Theory]
    [InlineData("KEDR ручка HRoz-06.103-AL-AB", "AL", true)]
    [InlineData("KEDR ручка HRoz-06.103-АL-AB", "AL", true)]
    [InlineData("KEDR ручка HRoz-06.103-al-AB", "AL", true)]
    [InlineData("KEDR ручка HRoz-06.103-CR", "AL", false)]
    [InlineData("Ручка антична бронза", "антична бронза", true)]
    [InlineData("Ручка", " ", false)]
    public void Mentions_FindsTheCodeWithLookAlikeLettersAndCase(string name, string code, bool expected)
        => Assert.Equal(expected, ProductAttributeRecognizer.Mentions(name, code));
}
