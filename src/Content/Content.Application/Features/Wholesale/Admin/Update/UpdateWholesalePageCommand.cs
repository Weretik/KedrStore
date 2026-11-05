using Content.Application.Features.Wholesale.Common;

namespace Content.Application.Features.Wholesale.Admin.Update;

/// <summary>Replaces both languages at once; exactly one <c>uk</c> and one <c>ru</c> text.</summary>
public sealed record UpdateWholesalePageCommand(IReadOnlyList<WholesaleTextDto> Texts)
    : ICommand<Result<AdminWholesalePage>>;
