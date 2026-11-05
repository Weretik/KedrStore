using Catalog.Contracts.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.SetForProduct;

/// <summary>
/// «Перенести в іншу групу», «Видалити з групи», or back to automatic. A moved product is pinned: automation never
/// moves it again until a person returns it to automatic grouping. The target group must be in the product's category.
/// </summary>
public sealed record SetProductGroupCommand(int ProductId, SetProductGroupRequest Request)
    : ICommand<Result<AdminProductGroupMembershipDto>>;
