using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;

/// <summary>«Повернути на перевірку»: an accepted product counts as a problem again if it still has reasons.</summary>
public sealed record ReopenProductReviewCommand(int ProductId) : ICommand<Result<AdminProductAttributesDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
