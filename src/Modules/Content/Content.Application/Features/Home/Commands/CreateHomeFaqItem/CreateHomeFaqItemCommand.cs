using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Commands.CreateHomeFaqItem;

/// <summary>Adds a question to the end of the FAQ.</summary>
public sealed record CreateHomeFaqItemCommand(HomeFaqInput Texts) : ICommand<Result<AdminHomeBlocks>>;
