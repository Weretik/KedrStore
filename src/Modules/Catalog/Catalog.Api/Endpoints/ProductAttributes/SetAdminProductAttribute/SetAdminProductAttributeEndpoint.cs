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
