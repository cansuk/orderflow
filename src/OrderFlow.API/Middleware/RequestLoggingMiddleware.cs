using System.Diagnostics;

namespace OrderFlow.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var method = context.Request.Method;

        await _next(context);
        stopwatch.Stop();

        _logger.LogInformation("{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            method, requestPath, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
