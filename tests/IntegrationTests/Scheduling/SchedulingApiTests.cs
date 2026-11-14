using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestSupport.Api;

namespace IntegrationTests.Scheduling;

/// <summary>Spec platform/007 over HTTP: SC-07, SC-08, SC-09 (Admin only) and SC-10 (the protected tick).</summary>
public sealed class SchedulingApiTests
{
    private const string Tasks = "/api/admin/scheduling/tasks";
    private const string Tick = "/internal/scheduler/tick";

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    [InlineData("Seo", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Tasks_are_for_Admin_only(string? role, HttpStatusCode expected)
    {
        using var host = new SchedulingTestHost();
        using var client = role is null ? host.Factory.CreateClient() : host.Factory.CreateClientWithRole(role);

        Assert.Equal(expected, (await client.GetAsync(Tasks)).StatusCode);
        if (role is not "Admin")
            Assert.Equal(expected, (await client.PostAsync($"{Tasks}/stocks/run", null)).StatusCode);
    }

    [Fact]
    public async Task Admin_sees_every_task_changes_a_schedule_and_runs_one_now()
    {
        using var host = new SchedulingTestHost();
        using var admin = host.Factory.CreateClientWithRole("Admin");

        var list = Json(await admin.GetStringAsync(Tasks));
        Assert.Equal("outbox", list[0].GetProperty("key").GetString());
        Assert.Contains(list.EnumerateArray(), task => task.GetProperty("key").GetString() == "stocks");

        using var saved = await admin.PutAsJsonAsync($"{Tasks}/stocks/schedule", new
        {
            enabled = true,
            schedule = new { kind = "daily", times = new[] { "07:00", "13:00" }, days = new[] { 1, 2, 3, 4, 5 } }
        });
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.True(Json(await saved.Content.ReadAsStringAsync()).GetProperty("enabled").GetBoolean());

        using var invalid = await admin.PutAsJsonAsync($"{Tasks}/stocks/schedule",
            new { enabled = true, schedule = new { kind = "interval", everyMinutes = 2 } });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var outbox = await admin.PutAsJsonAsync($"{Tasks}/outbox/schedule",
            new { enabled = true, schedule = new { kind = "manual" } });
        Assert.Equal(HttpStatusCode.Conflict, outbox.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"{Tasks}/unknown/runs")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"{Tasks}/full-sync/run", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsync($"{Tasks}/full-sync/run", null)).StatusCode);
    }

    [Fact]
    public async Task Tick_accepts_only_the_scheduler_and_starts_a_requested_task()
    {
        using var host = new SchedulingTestHost();
        using var anonymous = host.Factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(Tick, null)).StatusCode);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "someone-else");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(Tick, null)).StatusCode);

        using var admin = host.Factory.CreateClientWithRole("Admin");
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"{Tasks}/product-list/run", null)).StatusCode);

        using var scheduler = host.Factory.CreateClient();
        scheduler.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", SchedulingTestHost.SchedulerToken);
        using var tick = await scheduler.PostAsync(Tick, null);

        Assert.Equal(HttpStatusCode.OK, tick.StatusCode);
        var started = Json(await tick.Content.ReadAsStringAsync()).GetProperty("started");
        Assert.Equal("product-list", Assert.Single(started.EnumerateArray()).GetString());
        Assert.Equal("product-list", Assert.Single(Assert.Single(host.Launcher.Launches)).Key);
    }

    [Fact]
    public async Task Tick_is_not_found_until_it_is_configured()
    {
        using var host = new SchedulingTestHost(tickConfigured: false);
        using var scheduler = host.Factory.CreateClient();
        scheduler.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", SchedulingTestHost.SchedulerToken);

        Assert.Equal(HttpStatusCode.NotFound, (await scheduler.PostAsync(Tick, null)).StatusCode);
    }

    private static JsonElement Json(string body) => JsonDocument.Parse(body).RootElement.Clone();
}
