using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Delete;

public sealed record DeleteHomeBrandCommand(long Id) : ICommand<Result<AdminHomeBlocks>>;
