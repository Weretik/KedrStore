using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

namespace Scheduling.Api.Endpoints.Tick;

/// <summary>The scheduler's own endpoint, outside <c>/api</c> and the public contract.</summary>
public static class TickEndpoints
{
    public static IEndpointRouteBuilder MapSchedulerTick(this IEndpointRouteBuilder endpoints)
    {
        RunSchedulerTickEndpoint.Map(endpoints);
        return endpoints;
    }
}
