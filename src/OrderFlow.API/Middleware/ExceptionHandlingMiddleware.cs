using System.Text.Json;
using FluentValidation;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            OrderNotFoundException ex => (StatusCodes.Status404NotFound, new ErrorResponse("NotFound", ex.Message)),
            InvalidOrderStateException ex => (StatusCodes.Status409Conflict, new ErrorResponse("Conflict", ex.Message)),
            ValidationException ex => (StatusCodes.Status400BadRequest, new ErrorResponse("ValidationError",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)))),
            _ => (StatusCodes.Status500InternalServerError, new ErrorResponse("InternalError", "An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

public sealed record ErrorResponse(string Type, string Message);
