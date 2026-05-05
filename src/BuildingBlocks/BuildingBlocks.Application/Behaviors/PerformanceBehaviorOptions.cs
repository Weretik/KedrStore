namespace BuildingBlocks.Application.Behaviors;

public sealed class PerformanceBehaviorOptions
{
    public const string SectionName = "Application:Performance";

    public int DefaultThresholdMs { get; init; } = 500;
    public int AuthThresholdMs { get; init; } = 1000;
}
