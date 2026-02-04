var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();

await app.RunStartupTasksAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseCors("SpaDev");
}
else
{
    app.UseExceptionHandler(opt => { });
    app.UseStatusCodePages();
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

