using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;

public sealed record SetSiteModelTextCommand(int ModelId, string Language, SetSiteModelTextRequest Request)
    : ICommand<Result<SiteModelDto>>;
