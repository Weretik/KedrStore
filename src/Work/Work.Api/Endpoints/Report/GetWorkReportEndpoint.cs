using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Report.Queries.GetWorkReport;

namespace Work.Api.Endpoints.Report;

/// <summary>The administrator reads anybody's report.</summary>
internal static class GetWorkReportEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("report", Handle)
            .WithName("getAdminWorkReport")
            .Produces<WorkReportDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    // Missing values bind as default, as they did in the controller; the validator answers 400.
    private static async Task<IResult> Handle(
        ISender sender, HttpContext http, CancellationToken cancellationToken,
        [FromQuery] Guid? userId = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
        => (await sender.Send(new GetWorkReportQuery(userId ?? default, from ?? default, to ?? default),
            cancellationToken)).ToHttpResult(http);
}
