using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.ReopenReview;

/// <summary>«Повернути на перевірку»: an accepted product counts as a problem again if it still has reasons.</summary>
public sealed record ReopenProductReviewCommand(int ProductId) : ICommand<Result<AdminProductAttributesDto>>;
