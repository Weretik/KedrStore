using BuildingBlocks.Application.Snapshots;

namespace UnitTests.BuildingBlocks;

/// <summary>The snapshot version behind the offline app's ETag (spec platform/004, R-003).</summary>
public sealed class SnapshotVersionTests
{
    private sealed record Row(int Id, decimal Amount);

    [Fact]
    public void SameContent_GivesTheSameVersion()
    {
        var first = SnapshotVersion.Compute(new[] { new Row(1, 150m), new Row(2, 80m) });
        var second = SnapshotVersion.Compute(new[] { new Row(1, 150m), new Row(2, 80m) });

        Assert.Equal(first, second);
        Assert.Matches("^[0-9a-f]{32}$", first);
    }

    [Fact]
    public void ChangedAmount_GivesANewVersion()
    {
        var before = SnapshotVersion.Compute(new[] { new Row(1, 150m) });
        var after = SnapshotVersion.Compute(new[] { new Row(1, 175m) });

        Assert.NotEqual(before, after);
    }
}
