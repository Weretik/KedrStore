using Microsoft.Extensions.Options;
using Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;

namespace Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

/// <summary>
/// <c>POST /internal/scheduler/tick</c>: one minute of the scheduler (platform/007 R-04, R-10). Not part of the public
/// contract; only Cloud Scheduler's OIDC token passes.
/// </summary>
internal static class RunSchedulerTickEndpoint
{
    public const string Route = "/internal/scheduler/tick";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(Route, Handle).AllowAnonymous().ExcludeFromDescription();

    private static async Task<IResult> Handle(HttpContext http, IOptions<SchedulerTickOptions> options,
        ISchedulerCallerCheck callers, ISender sender, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured) return Results.NotFound();
        var header = http.Request.Headers.Authorization.ToString();
        const string bearer = "Bearer ";
        if (!header.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
            || !await callers.IsSchedulerAsync(header[bearer.Length..].Trim(), settings))
            return Results.Unauthorized();

        return (await sender.Send(new RunSchedulerTickCommand(), cancellationToken)).ToHttpResult(http);
    }
}
