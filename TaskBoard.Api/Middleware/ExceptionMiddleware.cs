using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace TaskBoard.Api.Middleware;

public sealed class ExceptionMiddleware 
{

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        
        _next = next;
        _logger = logger;

    }

    public async Task Invoke(HttpContext ctx)
    {
        
        try
        {

            await _next(ctx);

        }
        catch (KeyNotFoundException ex)
        {

            await WriteProblem(ctx, 404, "Not found", ex.Message);

        }
        catch (ArgumentException ex)
        {

            await WriteProblem(ctx, 400, "Invalid request", ex.Message);

        }
        catch (InvalidOperationException ex)
        {

            await WriteProblem(ctx, 409, "Conflict", ex.Message);

        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Unhandled exception");

            await WriteProblem(ctx, 500, "Internal server error", "Unexpected error.");

        }

    }

    private static Task WriteProblem(HttpContext ctx, int status, string title, string detail)
    {

        ctx.Response.StatusCode = status;

        return ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {

            Status = status,

            Title = title,

            Detail = detail

        });

    }





}
