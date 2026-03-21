using Microsoft.AspNetCore.Components.Authorization;
using TaskBoard.UI.Services.Api;
using TaskBoard.UI.Services.Auth;
using TaskBoard.UI.Services.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<AuthStateService>());

builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient<AuthClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Api:BaseUrl"]!);
});

builder.Services.AddHttpClient<TasksClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Api:BaseUrl"]!);
})
.AddHttpMessageHandler<AuthHeaderHandler>();