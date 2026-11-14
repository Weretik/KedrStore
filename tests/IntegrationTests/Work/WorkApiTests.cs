using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestSupport.Api;
using Media.Application.Abstractions.Persistence;
using Media.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Work.Application.Abstractions.Persistence;
using Work.Infrastructure.Persistence;

namespace IntegrationTests.Work;

/// <summary>Spec work/001 over HTTP on in-memory stores: own board, admin task, activity log, report.</summary>
public sealed class WorkApiTests
{
    private static readonly string Today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
    private static readonly string Yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1).ToString("yyyy-MM-dd");
    private static readonly string Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).ToString("yyyy-MM-dd");

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Seo", HttpStatusCode.OK)]
    [InlineData("Manager", HttpStatusCode.OK)]
    public async Task Board_IsForStaff(string? role, HttpStatusCode expected)
    {
        using var factory = CreateFactory();
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithUser(role, Guid.NewGuid());

        Assert.Equal(expected, (await client.GetAsync("/api/work/tasks")).StatusCode);
    }

    [Fact]
    public async Task Seo_KeepsOwnBoard_AndCannotTouchAnotherPersonsTask()
    {
        using var factory = CreateFactory();
        using var seo = factory.CreateClientWithUser("Seo", Guid.NewGuid());
        using var manager = factory.CreateClientWithUser("Manager", Guid.NewGuid());

        var id = await CreateTask(seo, "Тексти для 10 категорій ручок");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await seo.PostAsJsonAsync("/api/work/tasks", new { title = "  " })).StatusCode);

        using var started = await seo.PutAsJsonAsync($"/api/work/tasks/{id}/status", new { status = "InProgress" });
        Assert.Equal("InProgress", Json(await started.Content.ReadAsStringAsync()).GetProperty("status").GetString());
        using var done = await seo.PutAsJsonAsync($"/api/work/tasks/{id}/status", new { status = "Done", result = "10 текстів UK і RU" });
        var doneTask = Json(await done.Content.ReadAsStringAsync());
        Assert.Equal("10 текстів UK і RU", doneTask.GetProperty("result").GetString());
        Assert.Equal(HttpStatusCode.BadRequest,
            (await seo.PutAsJsonAsync($"/api/work/tasks/{id}/status", new { status = "Finished" })).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await manager.PutAsJsonAsync($"/api/work/tasks/{id}", new { title = "Чужа" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.DeleteAsync($"/api/work/tasks/{id}")).StatusCode);
        Assert.Equal(0, Json(await manager.GetStringAsync("/api/work/tasks")).GetArrayLength());
        Assert.Equal(1, Json(await seo.GetStringAsync("/api/work/tasks")).GetArrayLength());
    }

    [Fact]
    public async Task Admin_GivesATask_AndReadsTheReportWithAutomaticActivity()
    {
        using var factory = CreateFactory();
        var seoId = Guid.NewGuid();
        using var seo = factory.CreateClientWithUser("Seo", seoId);
        using var admin = factory.CreateClientWithUser("Admin", Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden,
            (await seo.PostAsJsonAsync("/api/admin/work/tasks", new { ownerUserId = seoId, title = "Сам собі" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await seo.GetAsync($"/api/admin/work/report?userId={seoId}&from={Today}&to={Today}")).StatusCode);

        using var given = await admin.PostAsJsonAsync("/api/admin/work/tasks", new { ownerUserId = seoId, title = "Перевірити SEO моделей" });
        Assert.Equal(HttpStatusCode.OK, given.StatusCode);
        var board = Json(await seo.GetStringAsync("/api/work/tasks"));
        Assert.False(Assert.Single(board.EnumerateArray()).GetProperty("createdByOwner").GetBoolean());

        // Two saves of one folder and one failed request: one activity line with count 2.
        using var folder = await seo.PostAsJsonAsync("/api/admin/media/folders", new { name = "SEO", parentId = (long?)null });
        var folderId = Json(await folder.Content.ReadAsStringAsync()).GetProperty("id").GetInt64();
        await seo.PutAsJsonAsync($"/api/admin/media/folders/{folderId}", new { name = "SEO 1", parentId = (long?)null });
        await seo.PutAsJsonAsync($"/api/admin/media/folders/{folderId}", new { name = "SEO 2", parentId = (long?)null });
        await seo.PutAsJsonAsync("/api/admin/media/folders/999999", new { name = "Немає", parentId = (long?)null });

        var report = Json(await admin.GetStringAsync($"/api/admin/work/report?userId={seoId}&from={Yesterday}&to={Tomorrow}"));
        Assert.Equal(1, report.GetProperty("openTasks").GetArrayLength());
        var activity = report.GetProperty("activity").EnumerateArray().ToList();
        Assert.Equal(2, activity.Count);
        var update = activity.Single(entry => entry.GetProperty("operation").GetString() == "Update");
        Assert.Equal("MediaFolders", update.GetProperty("area").GetString());
        Assert.Equal($"id={folderId}", update.GetProperty("target").GetString());
        Assert.Equal(2, update.GetProperty("count").GetInt32());

        var own = Json(await seo.GetStringAsync($"/api/work/report?from={Yesterday}&to={Tomorrow}"));
        Assert.Equal(2, own.GetProperty("activity").GetArrayLength());
        Assert.Equal(HttpStatusCode.BadRequest,
            (await seo.GetAsync($"/api/work/report?from={Today}&to={Yesterday}")).StatusCode);
    }

    [Fact]
    public async Task Task_KeepsDueDateAndSteps_AndMarksOverdue()
    {
        using var factory = CreateFactory();
        using var seo = factory.CreateClientWithUser("Seo", Guid.NewGuid());

        using var created = await seo.PostAsJsonAsync("/api/work/tasks", new
        {
            title = "Категорія «Петлі»",
            dueDate = Yesterday,
            steps = new[] { new { title = "SEO-заголовок", isDone = false }, new { title = "Опис", isDone = false } }
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var task = Json(await created.Content.ReadAsStringAsync());
        var id = task.GetProperty("id").GetInt64();
        Assert.True(task.GetProperty("isOverdue").GetBoolean());
        Assert.Equal(2, task.GetProperty("steps").GetArrayLength());

        using var ticked = await seo.PutAsJsonAsync($"/api/work/tasks/{id}/steps/1", new { isDone = true });
        Assert.True(Json(await ticked.Content.ReadAsStringAsync()).GetProperty("steps")[1].GetProperty("isDone").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound,
            (await seo.PutAsJsonAsync($"/api/work/tasks/{id}/steps/5", new { isDone = true })).StatusCode);

        using var moved = await seo.PutAsJsonAsync($"/api/work/tasks/{id}", new { title = "Категорія «Петлі»", dueDate = Tomorrow });
        var edited = Json(await moved.Content.ReadAsStringAsync());
        Assert.False(edited.GetProperty("isOverdue").GetBoolean());
        Assert.Equal(0, edited.GetProperty("steps").GetArrayLength());
    }

    private static async Task<long> CreateTask(HttpClient client, string title)
    {
        using var response = await client.PostAsJsonAsync("/api/work/tasks", new { title });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return Json(await response.Content.ReadAsStringAsync()).GetProperty("id").GetInt64();
    }

    private static JsonElement Json(string body) => JsonDocument.Parse(body).RootElement.Clone();

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var work = new WorkDbContext(new DbContextOptionsBuilder<WorkDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var media = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<IReadWorkDbContext>();
                services.RemoveAll<WorkDbContext>();
                services.AddSingleton(work);
                services.AddSingleton<IReadWorkDbContext>(work);
                services.RemoveAll<IReadMediaDbContext>();
                services.RemoveAll<MediaDbContext>();
                services.AddSingleton(media);
                services.AddSingleton<IReadMediaDbContext>(media);
            });
        });
    }
}
