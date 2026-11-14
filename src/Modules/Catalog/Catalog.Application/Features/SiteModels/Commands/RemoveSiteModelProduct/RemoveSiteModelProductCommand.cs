using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;

public sealed record RemoveSiteModelProductCommand(int ModelId, int ProductId) : ICommand<Result<SiteModelDto>>;
