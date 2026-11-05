using Catalog.Application.Features.Products.PublicListing;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace UnitTests.Catalog.Attributes;

/// <summary>Catalog attributes 006, phase 5: filter syntax, one card per group, and facet counts.</summary>
public sealed class PublicListingTests
{
    private static readonly AttributeId Color = AttributeId.Create(1);
    private static readonly AttributeId Side = AttributeId.Create(2);
    private static readonly AttributeValueId Chrome = AttributeValueId.Create(10);
    private static readonly AttributeValueId Black = AttributeValueId.Create(11);
    private static readonly AttributeValueId Left = AttributeValueId.Create(20);
    private static readonly AttributeValueId Right = AttributeValueId.Create(21);

    [Fact]
    public void Parse_MergesEntriesOfOneAttribute_AndRefusesBrokenOnes()
    {
        var parsed = AttributeFilters.Parse(["kolir:khrom", " Kolir : chornyi,khrom ", "storona:liva", ""])!;

        Assert.Equal(["chornyi", "khrom"], parsed["kolir"]);
        Assert.Equal(["liva"], parsed["storona"]);
        Assert.Empty(AttributeFilters.Parse(null)!);
        Assert.Null(AttributeFilters.Parse(["kolir"]));
        Assert.Null(AttributeFilters.Parse([":khrom"]));
        Assert.Null(AttributeFilters.Parse(["kolir:"]));
        Assert.Null(AttributeFilters.Parse([$"kolir:{string.Join(',', Enumerable.Range(0, 51))}"]));
    }

    [Fact]
    public void Collapse_KeepsTheFirstPlace_AndShowsTheMainProductWhenItMatches()
    {
        var cards = ProductCards.Collapse([
            new CardCandidate(5, 1, 6), new CardCandidate(9, null, null), new CardCandidate(6, 1, 6),
            new CardCandidate(7, 2, 99), new CardCandidate(8, 2, 99)
        ]);

        Assert.Equal([6, 9, 7], cards.Select(card => card.ProductId));
    }

    [Fact]
    public void Count_AppliesOtherAttributes_CountsCardsOnce()
    {
        // Group 1: products 1 (chrome, left) and 2 (black, left); product 3 alone (chrome, right).
        CardCandidate[] products = [new(1, 1, 1), new(2, 1, 1), new(3, null, null)];
        var values = new[]
        {
            (1, Color, Chrome), (1, Side, Left), (2, Color, Black), (2, Side, Left), (3, Color, Chrome), (3, Side, Right)
        }.ToLookup(item => item.Item1, item => (item.Item2, item.Item3));
        AttributeId[] attributes = [Color, Side];

        var none = FacetCounter.Count(products, values, [], attributes);
        Assert.Equal(2, none.TotalCards);
        Assert.Equal(2, none.ByValue[(Color, Chrome)]);
        Assert.Equal(1, none.ByValue[(Color, Black)]);
        Assert.Equal(1, none.ByValue[(Side, Left)]);

        var chrome = FacetCounter.Count(products, values, [new AttributeFilter(Color, "kolir", [Chrome])], attributes);
        Assert.Equal(2, chrome.TotalCards);
        Assert.Equal(1, chrome.ByValue[(Color, Black)]);
        Assert.Equal(1, chrome.ByValue[(Side, Right)]);

        var chromeRight = FacetCounter.Count(products, values,
            [new AttributeFilter(Color, "kolir", [Chrome]), new AttributeFilter(Side, "storona", [Right])], attributes);
        Assert.Equal(1, chromeRight.TotalCards);
        Assert.False(chromeRight.ByValue.ContainsKey((Color, Black)));
        Assert.Equal(1, chromeRight.ByValue[(Side, Left)]);
    }

    [Fact]
    public void DefaultSwatch_FillsOnlyAnEmptyValueNobodyEdited()
    {
        var now = DateTimeOffset.UtcNow;
        var plain = AttributeValue.Create(Color, "CR", "Хром", "khrom", 1, now);
        var edited = AttributeValue.CreateManual(Color, "BLACK", "Чорний", null, "chornyi", 2, null, null, now);

        Assert.True(plain.ApplyDefaultSwatch("#C8CCD0", now));
        Assert.False(plain.ApplyDefaultSwatch("#000000", now));
        Assert.Equal("#C8CCD0", plain.SwatchHex);
        Assert.False(edited.ApplyDefaultSwatch("#111111", now));
        Assert.Null(edited.SwatchHex);
    }
}
