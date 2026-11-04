using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Brands.Reorder;

/// <summary>The new site order: every brand ID exactly once, first shown first.</summary>
public sealed record ReorderHomeBrandsCommand(IReadOnlyList<long> Ids) : ICommand<Result<AdminHomeBlocks>>;
