using Content.Application.Features.Documents.Queries.GetLegalDocumentVersion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

internal static class GetAdminLegalDocumentVersionEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("versions/{id:long}", Handle)
            .WithName("getAdminLegalDocumentVersion")
            .Produces<LegalDocumentVersionResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetLegalDocumentVersionQuery(id), cancellationToken))
            .Map(LegalDocumentResponseMapper.ToResponse)
            .ToHttpResult(http);
}
