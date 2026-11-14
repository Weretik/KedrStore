namespace Content.Application.Features.Home.Shared;

internal static class HomeInputs
{
    public static HomeFaqTexts ToDomain(HomeFaqInput input) =>
        new(input.QuestionUk ?? string.Empty, input.QuestionRu ?? string.Empty, input.AnswerUk ?? string.Empty,
            input.AnswerRu ?? string.Empty);
}
