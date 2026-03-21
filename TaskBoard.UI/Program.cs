using Microsoft.AspNetCore.Components.Authorization;
using TaskBoard.UI.Services.Auth;
using TaskBoard.UI.Services.Api;
using TaskBoard.UI.Services.Http;
using TaskBoard.UI.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Auth — všechno Scoped, vázané na circuit
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<AuthStateService>());

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// HTTP klienti
builder.Services.AddTransient<AuthHeaderHandler>();

var apiBase = builder.Configuration["ApiBaseUrl"] 
    ?? "https://localhost:7001";

builder.Services.AddHttpClient<AuthClient>(c =>
    c.BaseAddress = new Uri(apiBase));

builder.Services.AddHttpClient<TasksClient>(c =>
    c.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AuthHeaderHandler>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
