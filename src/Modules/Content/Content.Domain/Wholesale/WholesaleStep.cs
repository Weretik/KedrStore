namespace Content.Domain.Wholesale;

/// <summary>One step of «Як замовити».</summary>
public sealed class WholesaleStep
{
    public string Title { get; private set; } = null!;
    public string Text { get; private set; } = null!;

    private WholesaleStep() { }

    internal WholesaleStep(string title, string text)
    {
        Title = title;
        Text = text;
    }
}
