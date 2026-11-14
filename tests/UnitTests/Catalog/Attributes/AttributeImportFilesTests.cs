using Catalog.Application.Attributes.Import;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace UnitTests.Catalog.Attributes;

/// <summary>Catalog attributes 006, phase 1: file readers and review rules.</summary>
public sealed class AttributeImportFilesTests
{
    [Fact]
    public void CsvText_ReadsQuotedCommasDoubledQuotesAndLineBreaks()
    {
        var records = CsvText.ReadRecords(new StringReader("a,b\n1,\"x, \"\"y\"\"\nz\"\n")).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal(["1", "x, \"y\"\nz"], records[1].Fields);
    }

    [Theory]
    [InlineData("100×75×2.5 mm", "100x75x2.5mm")]
    [InlineData(" 35T×35 mm ", "35tx35mm")]
    public void SizeCode_IsStable(string text, string code) => Assert.Equal(code, AttributeCsvFile.SizeCode(text));

    [Fact]
    public void Csv_NullLiteralIsEmpty_AndNoNameAddsReason()
    {
        const string csv = """
            product_id,base_name,size,size_length,size_width,size_thickness,size_diameter,color_code,color_secondary_code,side,extra_attributes,needs_review,review_reason
            7,Ручка,null,null,null,null,null,AB,null,null,"{""attributes"":{""branding"":[{""code"":""NO_NAME""}]},""review_reason_codes"":[]}",false,null
            """;

        var result = AttributeCsvFile.Read(new StringReader(csv));

        var product = Assert.Single(result.Products);
        Assert.Empty(result.Errors);
        Assert.Equal(2, product.Values.Count); // color AB + branding NO_NAME
        Assert.Equal(AttributeReviewReason.NoName, Assert.Single(product.Reasons).Code);
    }

    [Fact]
    public void AcceptedReview_StaysAccepted_UntilReopened()
    {
        var now = DateTimeOffset.UtcNow;
        var review = ProductAttributeReview.Create(ProductId.From(1), "Ручка", [new("unresolved_color", null)], now);
        review.Accept("admin@classkedr.com.ua", now);

        review.ApplyAutomatic("Ручка", [new("unresolved_color", null), new("no_name", null)], now);
        Assert.Equal(AttributeReviewState.Accepted, review.State);

        review.Reopen(now);
        Assert.Equal(AttributeReviewState.NeedsReview, review.State);
    }

    [Fact]
    public void ManualValue_IsNotChangedByAutomaticSources()
    {
        var now = DateTimeOffset.UtcNow;
        var value = ProductAttributeValue.CreateManual(ProductId.From(1), AttributeId.Create(1), 0,
            AttributeValueId.Create(5), null, now);

        Assert.False(value.ApplyAutomatic(AttributeValueId.Create(6), null, true, AttributeValueSource.Parser, now));
        Assert.Equal(AttributeValueId.Create(5), value.ValueId);
    }
}
