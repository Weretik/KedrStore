using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.Faq.Create;

/// <summary>Adds a question to the end of the FAQ.</summary>
public sealed record CreateHomeFaqItemCommand(HomeFaqInput Texts) : ICommand<Result<AdminHomeBlocks>>;
