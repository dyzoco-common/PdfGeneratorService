using System.Text.Json;
using PdfGeneratorService.Application.Responses;
using PdfGeneratorService.Domain.Exceptions;
using ValidationException = PdfGeneratorService.Domain.Exceptions.ValidationException;

namespace PdfGeneratorService.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            await WriteErrorAsync(context, 400, "Validation error", ex.Message,
                ex.Errors.Select(e => new ApiFieldError(e.Field, e.Message)));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            await WriteErrorAsync(context, 422, "Domain error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var message = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred. Please try again later.";

            await WriteErrorAsync(context, 500, "Internal server error", message);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string status,
        string message,
        IEnumerable<ApiFieldError>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Status = "error",
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
