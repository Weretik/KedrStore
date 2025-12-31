using BuildingBlocks.Application.Behaviors;

namespace BuildingBlocks.Application.DependencyInjection;

public static class MediatorPipeline
{
    public static Type[] PipelineBehaviors =>
    [
        typeof(RequestLoggingBehavior<,>),
        typeof(PerformanceBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(ExceptionBehavior<,>),
        typeof(DomainEventDispatcherBehavior<,>)
    ];
}
