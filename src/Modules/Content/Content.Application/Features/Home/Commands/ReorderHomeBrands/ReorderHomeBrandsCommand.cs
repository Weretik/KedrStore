using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.ReorderHomeBrands;

/// <summary>The new site order: every brand ID exactly once, first shown first.</summary>
public sealed record ReorderHomeBrandsCommand(IReadOnlyList<long> Ids) : ICommand<Result<AdminHomeBlocks>>;
