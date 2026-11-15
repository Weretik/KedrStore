using Content.Application.Features.Documents.Queries.GetLegalDocuments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Documents.Shared;

namespace Content.Api.Endpoints.Documents.GetAdminLegalDocuments;

internal static class GetAdminLegalDocumentsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminLegalDocuments")
            .WithSummary("List the documents with their published edition and waiting draft")
            .WithDescription("Requires `CanManageContent` (Manager, Seo, or Admin). Always four items, in a fixed order.")
            .Produces<IReadOnlyList<LegalDocumentOverviewResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLegalDocumentsQuery(), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<LegalDocumentOverviewResponse>)items.Select(LegalDocumentResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
