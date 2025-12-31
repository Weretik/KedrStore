using Catalog.Api.Contracts.Products;

namespace Catalog.Api.Endpoints.Products;

public sealed class GetProductsEndpoint(ISender sender)
    : EndpointBaseAsync
        .WithRequest<GetProductsRequest>
        .WithActionResult
{
    [HttpGet("/api/catalog/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1️⃣ API → Application (через mapper)
        var query = request.ToQuery();

        // 2️⃣ Выполняем use case
        var result = await sender.Send(query, cancellationToken);

        // 3️⃣ HTTP-ответ
        if (result.IsSuccess)
            return Ok(result.Value);

        // временно,заменить на ProblemDetails
        return BadRequest(result.Errors);
    }
}

