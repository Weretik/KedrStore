using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Work.Api.Endpoints.Activity;
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
        Assert.Equal("Categories", WorkActivityMiddleware.AreaOf("AdminCategories"));
        Assert.Equal("id=12, lang=uk", WorkActivityMiddleware.TargetOf(new Dictionary<string, object?>
        {
            ["controller"] = "AdminCategories",
            ["action"] = "UpdateContent",
            ["lang"] = "uk",
            ["id"] = "12"
        }));
        Assert.True(WorkActivityMiddleware.IsChange("PUT"));
        Assert.False(WorkActivityMiddleware.IsChange("GET"));
    }

    [Fact]
    public void Middleware_Names_A_Minimal_Api_Admin_Endpoint_By_Its_Tag_And_Name()
    {
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(
            new TagsAttribute("AdminCategories"),
            new EndpointNameMetadata("updateAdminCategoryContent"),
            typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!), "test");

        var source = WorkActivityMiddleware.Describe(endpoint);

        Assert.NotNull(source);
        Assert.Equal("AdminCategories", source.Group);
        Assert.Equal("updateAdminCategoryContent", source.Operation);
        Assert.True(WorkActivityMiddleware.IsLogged(source));
    }

    [Fact]
    public void Middleware_Names_An_Unnamed_Endpoint_By_Its_Class()
    {
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(
            new TagsAttribute("AdminCategories"),
            typeof(ChangeAdminCategorySlugEndpoint).GetMethod(nameof(ChangeAdminCategorySlugEndpoint.Handle))!), "test");

        var source = WorkActivityMiddleware.Describe(endpoint);

        Assert.NotNull(source);
        Assert.Equal("ChangeAdminCategorySlug", source.Operation);
    }

    [Fact]
    public void Middleware_Skips_An_Endpoint_Without_A_Tag() =>
        Assert.Null(WorkActivityMiddleware.Describe(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new EndpointNameMetadata("x")), "test")));

    /// <summary>Stands for a generated endpoint class without a name.</summary>
    private static class ChangeAdminCategorySlugEndpoint
    {
        public static void Handle() { }
    }
}
