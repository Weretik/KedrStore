using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Rename;

public sealed record RenameSiteModelCommand(int ModelId, RenameSiteModelRequest Request) : ICommand<Result<SiteModelDto>>;
