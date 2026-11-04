using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Update;

public sealed record UpdateHomeBrandCommand(long Id, HomeBrandInput Brand) : ICommand<Result<AdminHomeBlocks>>;
