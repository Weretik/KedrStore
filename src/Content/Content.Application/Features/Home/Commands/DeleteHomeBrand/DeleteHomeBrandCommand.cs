using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.DeleteHomeBrand;

public sealed record DeleteHomeBrandCommand(long Id) : ICommand<Result<AdminHomeBlocks>>;
