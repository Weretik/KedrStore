using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Rename;

/// <summary>Renames a group. The slug stays; only an Admin changes it (it is part of the site markup).</summary>
public sealed record RenameProductGroupCommand(int GroupId, RenameProductGroupRequest Request)
    : ICommand<Result<AdminProductGroupDto>>;
