using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.AcceptReview;

/// <summary>
/// «Прийняти як є»: the product stops counting as a problem. Permanent (owner decision 2026-11-05): automation keeps
/// it accepted, only «Повернути на перевірку» undoes it.
/// </summary>
public sealed record AcceptProductReviewCommand(int ProductId) : ICommand<Result<AdminProductAttributesDto>>;
