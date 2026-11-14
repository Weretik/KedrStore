using System.Net.Http.Headers;
using System.Net.Http.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Scheduling.Application.Abstractions;

namespace Scheduling.Infrastructure.Integrations.CloudRun;

/// <summary>
/// Starts one execution of the tasks job through the Cloud Run Admin API (<c>jobs:run</c>) with the arguments
/// <c>--job=tasks --tasks=key:runId,...</c> (the override replaces the job's own arguments). The API's own service account needs <c>roles/run.jobsExecutorWithOverrides</c> on
/// that job (platform/007 EN-01).
/// </summary>
internal sealed class CloudRunTaskJobLauncher(HttpClient http, IOptions<CloudRunJobOptions> options) : ITaskJobLauncher
{
    private const string Scope = "https://www.googleapis.com/auth/cloud-platform";

    public static string Argument(IEnumerable<TaskLaunch> launches) =>
        "--tasks=" + string.Join(',', launches.Select(x => $"{x.Key}:{x.RunId}"));

    public async Task<string?> LaunchAsync(IReadOnlyList<TaskLaunch> launches, CancellationToken cancellationToken)
    {
        var job = options.Value;
        if (!job.IsConfigured) return "запуск задач у Google Cloud не налаштований (Scheduler:Job).";

        var credential = (await GoogleCredential.GetApplicationDefaultAsync(cancellationToken)).CreateScoped(Scope);
        var token = await ((ITokenAccess)credential).GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
        var url = $"https://run.googleapis.com/v2/projects/{job.Project}/locations/{job.Region}/jobs/{job.Name}:run";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new { overrides = new { containerOverrides = new[] { new { args = new[] { "--job=tasks", Argument(launches) } } } } })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return $"Cloud Run відповів {(int)response.StatusCode}: {body[..Math.Min(body.Length, 300)]}";
    }
}
