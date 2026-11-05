using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.RemoveProduct;

public sealed record RemoveSiteModelProductCommand(int ModelId, int ProductId) : ICommand<Result<SiteModelDto>>;
