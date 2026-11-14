using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Commands.SetProductGroupMain;

/// <summary>
/// A person chooses the main product («Зробити головним»). It must be in the group and on the site; it stays main
/// until it is deleted or taken off the site.
/// </summary>
public sealed record SetProductGroupMainCommand(int GroupId, SetProductGroupMainRequest Request)
    : ICommand<Result<AdminProductGroupDto>>;
