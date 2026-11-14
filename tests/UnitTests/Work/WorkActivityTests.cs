using Work.Api.Features.Activity;
using Work.Domain.Activity;

namespace UnitTests.Work;

public sealed class WorkActivityTests
{
    private static readonly Guid User = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);

    private static WorkActivity Entry(DateTimeOffset at, string target = "id=12, lang=uk", Guid? user = null) =>
        WorkActivity.Record(user ?? User, "seo@kedr.ua", "Categories", "UpdateContent", "put", target, at).Value;

    [Fact]
    public void Repeat_Within_Ten_Minutes_Merges()
    {
        var first = Entry(Now);
        var repeat = Entry(Now.AddMinutes(9));

        Assert.True(first.CanMerge(repeat));
        first.Repeat(repeat.OccurredAt);
        Assert.Equal(2, first.Count);
        Assert.Equal(Now, first.FirstAt);
        Assert.Equal(Now.AddMinutes(9), first.OccurredAt);
        Assert.Equal("PUT", first.Method);
    }

    [Fact]
    public void Another_Target_User_Or_Later_Time_Is_A_New_Entry()
    {
        var first = Entry(Now);

        Assert.False(first.CanMerge(Entry(Now.AddMinutes(11))));
        Assert.False(first.CanMerge(Entry(Now.AddMinutes(1), "id=13, lang=uk")));
        Assert.False(first.CanMerge(Entry(Now.AddMinutes(1), user: Guid.NewGuid())));
    }

    [Fact]
    public void Filter_Names_The_Area_And_Target()
    {
        Assert.Equal("Categories", WorkActivityFilter.AreaOf("AdminCategories"));
        Assert.Equal("id=12, lang=uk", WorkActivityFilter.TargetOf(new Dictionary<string, object?>
        {
            ["controller"] = "AdminCategories",
            ["action"] = "UpdateContent",
            ["lang"] = "uk",
            ["id"] = "12"
        }));
        Assert.True(WorkActivityFilter.IsChange("PUT"));
        Assert.False(WorkActivityFilter.IsChange("GET"));
    }
}
