using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.UpdateHomeFaqItem;

public sealed record UpdateHomeFaqItemCommand(long Id, HomeFaqInput Texts) : ICommand<Result<AdminHomeBlocks>>;
