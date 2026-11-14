using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Queries.GetProblemReasons;

/// <summary>Review reasons of on-site products that need review, most frequent first (filter «За причиною»).</summary>
public sealed record GetProblemReasonsQuery : IQuery<Result<List<AdminProblemReasonDto>>>;
