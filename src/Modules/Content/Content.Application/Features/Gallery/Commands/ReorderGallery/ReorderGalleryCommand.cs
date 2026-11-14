using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.ReorderGallery;

/// <summary>The new site order: every card ID exactly once, first shown first.</summary>
public sealed record ReorderGalleryCommand(IReadOnlyList<long> Ids) : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
