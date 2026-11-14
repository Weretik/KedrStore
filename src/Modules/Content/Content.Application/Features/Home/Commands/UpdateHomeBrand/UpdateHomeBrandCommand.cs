using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeBrand;

public sealed record UpdateHomeBrandCommand(long Id, HomeBrandInput Brand) : ICommand<Result<AdminHomeBlocks>>;
