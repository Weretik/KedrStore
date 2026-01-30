using Host.Api.DependencyInjection.ServiceCollections;
using Host.Api.DependencyInjection.WebApplications.HostPipelines;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var corsPolicyName = builder.Configuration["Cors:PolicyName"] ?? "Spa";

var app = builder.Build();

await app.RunStartupTasksAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.AddHangfireRecurringJobs();
app.UseHangfireDashboard("/hangfire");

app.Run();

