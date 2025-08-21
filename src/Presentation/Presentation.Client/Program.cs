var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
