using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Reorder;

/// <summary>The new site order: every question ID exactly once, first shown first.</summary>
public sealed record ReorderHomeFaqCommand(IReadOnlyList<long> Ids) : ICommand<Result<AdminHomeBlocks>>;
