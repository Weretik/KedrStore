namespace Content.Domain.Wholesale;

/// <summary>One question for wholesale buyers.</summary>
public sealed class WholesaleFaqItem
{
    public string Question { get; private set; } = null!;
    public string Answer { get; private set; } = null!;

    private WholesaleFaqItem() { }

    internal WholesaleFaqItem(string question, string answer)
    {
        Question = question;
        Answer = answer;
    }
}
