var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<IState>(sp => sp.GetRequiredService<BurgerMenuState>());
builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<BurgerMenuState>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
