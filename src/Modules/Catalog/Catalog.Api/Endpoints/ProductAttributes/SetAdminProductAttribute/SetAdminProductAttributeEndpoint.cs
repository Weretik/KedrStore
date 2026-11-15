using Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.ProductAttributes.SetAdminProductAttribute;

internal static class SetAdminProductAttributeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("attributes/{attributeId:int}", Handle)
            .WithName("setAdminProductAttribute")
            .WithSummary("Set the values of one attribute by hand")
            .WithDescription("The values (in position order) become manual and locked: the import, the parser, and the 1C sync no longer change them. List attributes take `valueId` from the attribute's list, number attributes take `number`. Positions beyond the list are dropped. A change of color, size, or side rebuilds the variant groups.")
            .Produces<AdminProductAttributesDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Sets the values of one attribute by hand; they become manual and locked.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int productId, [FromRoute] int attributeId, [FromBody] SetProductAttributeRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new SetProductAttributeCommand(productId, attributeId, request),
            cancellationToken)).ToHttpResult(http);
}
