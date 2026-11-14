using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.FillSiteModelFromNames;

public sealed record FillSiteModelFromNamesCommand(int ModelId) : ICommand<Result<SiteModelDto>>;
