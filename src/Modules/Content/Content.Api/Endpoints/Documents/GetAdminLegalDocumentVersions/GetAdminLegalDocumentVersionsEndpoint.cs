using Content.Application.Features.Documents.Queries.GetLegalDocumentVersions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Documents.Shared;

namespace Content.Api.Endpoints.Documents.GetAdminLegalDocumentVersions;

internal static class GetAdminLegalDocumentVersionsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{kind}/versions", Handle)
            .WithName("getAdminLegalDocumentVersions")
            .Produces<IReadOnlyList<LegalDocumentVersionSummaryResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string kind, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLegalDocumentVersionsQuery(kind), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<LegalDocumentVersionSummaryResponse>)items.Select(LegalDocumentResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
