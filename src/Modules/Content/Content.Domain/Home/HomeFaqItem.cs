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

    public static DomainResult<HomeFaqItem> Create(HomeFaqTexts texts, int position, DateTimeOffset now)
    {
        var checkedPosition = HomeGuard.Position(position);
        if (checkedPosition.IsFailure)
            return checkedPosition.Error!;

        var item = new HomeFaqItem { Position = checkedPosition.Value, CreatedAt = now };
        var updated = item.Update(texts, now);
        return updated.IsSuccess ? item : updated.Error!;
    }

    public DomainResult Update(HomeFaqTexts texts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(texts);
        var questionUk = HomeGuard.Text(texts.QuestionUk, nameof(QuestionUk), QuestionMaxLength);
        var questionRu = HomeGuard.Text(texts.QuestionRu, nameof(QuestionRu), QuestionMaxLength);
        var answerUk = HomeGuard.Text(texts.AnswerUk, nameof(AnswerUk), AnswerMaxLength);
        var answerRu = HomeGuard.Text(texts.AnswerRu, nameof(AnswerRu), AnswerMaxLength);
        var valid = DomainResult.Combine(questionUk, questionRu, answerUk, answerRu);
        if (valid.IsFailure)
            return valid;

        QuestionUk = questionUk.Value;
        QuestionRu = questionRu.Value;
        AnswerUk = answerUk.Value;
        AnswerRu = answerRu.Value;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public DomainResult MoveTo(int position, DateTimeOffset now)
    {
        var checkedPosition = HomeGuard.Position(position);
        if (checkedPosition.IsFailure)
            return checkedPosition;
        if (checkedPosition.Value == Position)
            return DomainResult.Success();

        Position = position;
        UpdatedAt = now;
        return DomainResult.Success();
    }
}

/// <summary>Question and answer in both languages; all four are required.</summary>
public sealed record HomeFaqTexts(string QuestionUk, string QuestionRu, string AnswerUk, string AnswerRu);
