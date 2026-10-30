namespace Catalog.Domain.Entities;

/// <summary>One question and answer of a category content row.</summary>
public class CategoryFaqItem
{
    public const int QuestionMaxLength = 200;
    public const int AnswerMaxLength = 2_000;

    public long Id { get; private set; }
    public int SortOrder { get; private set; }
    public string Question { get; private set; } = null!;
    public string Answer { get; private set; } = null!;

    private CategoryFaqItem() { }

    internal static CategoryFaqItem Create(int sortOrder, string question, string answer) => new()
    {
        SortOrder = sortOrder,
        Question = question,
        Answer = answer
    };
}
