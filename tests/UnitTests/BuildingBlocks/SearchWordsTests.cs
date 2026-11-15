using BuildingBlocks.Application.Text;

namespace UnitTests.BuildingBlocks;

public sealed class SearchWordsTests
{
    [Fact]
    public void Patterns_SplitTheTextIntoEscapedWords()
        => Assert.Equal(["%ручка%", @"%50\%%", @"%a\_b%"], SearchWords.Patterns("  ручка\t50%   a_b "));

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Patterns_OfBlankText_AreEmpty(string? text) => Assert.Empty(SearchWords.Patterns(text));
}
