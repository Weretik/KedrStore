using Microsoft.Extensions.DependencyInjection;

namespace Host.Jobs;

/// <summary>
/// Container validation of the jobs host: a Singleton that holds a Scoped service (a captive DbContext) or a missing
/// registration fails when the job starts, in every environment.
/// </summary>
public static class JobsServiceProviderOptions
{
    public static ServiceProviderOptions Value { get; } = new() { ValidateScopes = true, ValidateOnBuild = true };
}
