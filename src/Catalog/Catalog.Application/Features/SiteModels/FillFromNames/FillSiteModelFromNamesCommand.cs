using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.FillFromNames;

public sealed record FillSiteModelFromNamesCommand(int ModelId) : ICommand<Result<SiteModelDto>>;
