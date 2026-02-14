using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace TaskBoard.Api.Security;

public sealed class KeyMiddleware : IMiddleware
{

    private const string HeaderName = "X-API-KEY";

    private readonly byte[] _expectedBytes;

    private readonly KeyOptions _keyOptions;

    public KeyMiddleware(IOptions<KeyOptions> keyOptions)
    {

        _keyOptions = keyOptions.Value;

        if (!string.IsNullOrEmpty(_keyOptions.ApiKey))
            _expectedBytes = Encoding.UTF8.GetBytes(_keyOptions.ApiKey);
        else
            _expectedBytes = Array.Empty<byte>();

    }
    

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey) || providedKey.Count != 1 || string.IsNullOrWhiteSpace(providedKey[0]))
        {

            await Unauthorized(context, $"Missing or invalid {HeaderName}.");

            return;

        }

        string? provided = providedKey[0]!;
        
        string expected = _keyOptions.ApiKey;

        if (string.IsNullOrEmpty(expected))
        {

            await Misconfigured(context);

            return;

        }

        var providedBytes = Encoding.UTF8.GetBytes(provided);

        if (providedBytes.Length != _expectedBytes.Length || !CryptographicOperations.FixedTimeEquals(providedBytes, _expectedBytes))
        {

            await Unauthorized(context, $"Missing or invalid {HeaderName}.");

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

    private Task Unauthorized(HttpContext ctx, string detail) =>
        WriteProblem(ctx, 401, "Unauthorized", detail);

    private Task Misconfigured(HttpContext ctx) =>
        WriteProblem(ctx, 500, "Server misconfiguration", "Security:ApiKey is not configured.");



}
