using BuildingBlocks.Application.Results;

namespace UnitTests.BuildingBlocks.Results;

public sealed class RowVersionsTests
{
    [Theory]
    [InlineData(null, 7u, false)]
    [InlineData(7u, 7u, false)]
    [InlineData(6u, 7u, true)]
    [InlineData(0u, 7u, true)]
    [InlineData(0u, 0u, false)]
    public void IsStale_OnlyWhenASentVersionDiffers(uint? sent, uint current, bool stale)
        => Assert.Equal(stale, RowVersions.IsStale(sent, current));

    [Fact]
    public void Changed_IsAConflictWithTheUkrainianMessage()
    {
        var result = RowVersions.Changed<int>();

        Assert.Equal(Ardalis.Result.ResultStatus.Conflict, result.Status);
        Assert.Equal(RowVersions.ChangedMessage, Assert.Single(result.Errors));
    }
}
