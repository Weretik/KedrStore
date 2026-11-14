using Content.Application.Features.Documents.Queries.GetLegalDocuments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

internal static class GetAdminLegalDocumentsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminLegalDocuments")
            .Produces<IReadOnlyList<LegalDocumentOverviewResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLegalDocumentsQuery(), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<LegalDocumentOverviewResponse>)items.Select(LegalDocumentResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
