using Sales.Application.Abstractions.Orders;

namespace Sales.Api.Endpoints.OrderWorkspace;

/// <summary>
/// The checkout 409 as <c>application/problem+json</c> that keeps <c>code</c> (<c>quote_changed</c> or
/// <c>idempotency_conflict</c>) and the fresh <c>quote</c> at the top level, where the admin reads them.
/// </summary>
internal static class WorkspaceConflictProblem
{
    public static IResult ToProblem(this WorkspaceOrderConflict conflict, HttpContext http)
    {
        var problem = ResultProblems.Create(http, StatusCodes.Status409Conflict, conflict.Code,
            conflict.Code == "quote_changed"
                ? "Ціни або залишки змінилися. Перевірте замовлення."
                : "Замовлення з цим ключем уже створене з іншими даними.");
        if (conflict.Quote is not null)
            problem.Extensions["quote"] = conflict.Quote;
        return TypedResults.Problem(problem);
    }
}
