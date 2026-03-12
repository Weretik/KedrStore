using Catalog.Application.Integrations.OneC.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

await app.RunStartupTasksAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
/*
    try
    {
        using var scope = app.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<SyncOneCCategoryJob>();

        await job.RunAsync("000005513", app.Lifetime.ApplicationStopping); // hardware
        await job.RunAsync("000007226", app.Lifetime.ApplicationStopping); // doors
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "SyncOneCCategoryJob failed during Development startup.");
    }
    */
}
else
{
    app.UseExceptionHandler(opt => { });
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

