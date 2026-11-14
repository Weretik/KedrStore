using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Report.Queries.GetWorkReport;
namespace Work.Api.Endpoints.Report.GetMyWorkReport;

internal static class GetMyWorkReportEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("report", Handle)
            .WithName("getMyWorkReport")
            .Produces<WorkReportDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    // Missing dates bind as default, as they did in the controller; the validator answers 400.
    private static async Task<IResult> Handle(
        ISender sender, HttpContext http, CancellationToken cancellationToken,
        [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
        => (await sender.Send(new GetWorkReportQuery(null, from ?? default, to ?? default), cancellationToken))
            .ToHttpResult(http);
}
