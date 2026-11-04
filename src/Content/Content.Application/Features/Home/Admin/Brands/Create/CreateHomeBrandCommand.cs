using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Create;

/// <summary>Adds a brand to the end of the list.</summary>
public sealed record CreateHomeBrandCommand(HomeBrandInput Brand) : ICommand<Result<AdminHomeBlocks>>;
