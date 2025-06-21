namespace Infrastructure.Shared.Services;

public class EnvironmentService(IWebHostEnvironment env) : IEnvironmentService
{
    public bool IsProduction() => env.IsProduction();
}
