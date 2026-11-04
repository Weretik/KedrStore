namespace Content.Domain.Home;

/// <summary>
/// One question of the home page FAQ in both site languages. The site shows questions by
/// <see cref="Position"/>, smallest first.
/// </summary>
public sealed class HomeFaqItem : IAggregateRoot
{
    public const int QuestionMaxLength = 200;
    public const int AnswerMaxLength = 2000;

    public long Id { get; private set; }
    public int Position { get; private set; }
    public string QuestionUk { get; private set; } = string.Empty;
    public string QuestionRu { get; private set; } = string.Empty;
    public string AnswerUk { get; private set; } = string.Empty;
    public string AnswerRu { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private HomeFaqItem() { }

    public static HomeFaqItem Create(HomeFaqTexts texts, int position, DateTimeOffset now)
    {
        var item = new HomeFaqItem { Position = HomeGuard.Position(position), CreatedAt = now };
        item.Update(texts, now);
        return item;
    }

    public void Update(HomeFaqTexts texts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(texts);
        QuestionUk = HomeGuard.Text(texts.QuestionUk, nameof(QuestionUk), QuestionMaxLength);
        QuestionRu = HomeGuard.Text(texts.QuestionRu, nameof(QuestionRu), QuestionMaxLength);
        AnswerUk = HomeGuard.Text(texts.AnswerUk, nameof(AnswerUk), AnswerMaxLength);
        AnswerRu = HomeGuard.Text(texts.AnswerRu, nameof(AnswerRu), AnswerMaxLength);
        UpdatedAt = now;
    }

    public void MoveTo(int position, DateTimeOffset now)
    {
        if (HomeGuard.Position(position) == Position)
            return;
        Position = position;
        UpdatedAt = now;
    }
}

/// <summary>Question and answer in both languages; all four are required.</summary>
public sealed record HomeFaqTexts(string QuestionUk, string QuestionRu, string AnswerUk, string AnswerRu);
