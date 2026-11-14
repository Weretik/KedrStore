using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.ReorderHomeFaq;

/// <summary>The new site order: every question ID exactly once, first shown first.</summary>
public sealed record ReorderHomeFaqCommand(IReadOnlyList<long> Ids) : ICommand<Result<AdminHomeBlocks>>;
