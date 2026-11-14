using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeBrand;

/// <summary>Adds a brand to the end of the list.</summary>
public sealed record CreateHomeBrandCommand(HomeBrandInput Brand) : ICommand<Result<AdminHomeBlocks>>;
