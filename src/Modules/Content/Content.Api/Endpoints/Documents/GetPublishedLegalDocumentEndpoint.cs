using Content.Application.Features.Documents.Queries.GetPublishedLegalDocument;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

internal static class GetPublishedLegalDocumentEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/documents/{kind}", Handle)
            .WithName("getPublishedLegalDocument")
            .Produces<PublishedLegalDocumentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary><c>kind</c>: <c>public-offer</c>, <c>privacy-policy</c>, <c>returns-policy</c> or <c>delivery-payment</c>.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string kind, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublishedLegalDocumentQuery(kind, lang), cancellationToken);
        return result.Map(LegalDocumentResponseMapper.ToResponse).ToHttpResult(http);
    }
}
