using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TaskBoard.Api.Security;

public sealed class KeyMiddleware : IMiddleware
{

    private const string HeaderName = "X-API-KEY";
    private readonly KeyOptions _keyOptions;

    public KeyMiddleware(IOptions<KeyOptions> keyOptions) 
        => _keyOptions = keyOptions.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            
            await next(context);
            return;

        }

        if (string.IsNullOrWhiteSpace(_keyOptions.ApiKey))
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "API key is not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey))
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", $"Missing header '{HeaderName}'.");
            return;
        }

        if (!string.Equals(providedKey.ToString(), _keyOptions.ApiKey, StringComparison.Ordinal))
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "Invalid API key.");
            return;
        }

       await next(context);

    }


    public static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        
        context.Response.StatusCode = status;

        context.Response.ContentType = "application/problem+json; charset=utf-8";

        var problem = new ProblemDetails
        {

            Title = title,

            Detail = detail,
            
            Status = status

        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));

    }



}
