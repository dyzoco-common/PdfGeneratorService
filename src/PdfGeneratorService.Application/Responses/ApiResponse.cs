namespace PdfGeneratorService.Application.Responses;

public sealed class ApiResponse<T>
{
    public int StatusCode { get; init; }
    public string Status { get; init; } = "success";
    public string Message { get; init; } = string.Empty;
    public T? Body { get; init; }

    public static ApiResponse<T> Success(T body, string message = "Operation completed successfully", int statusCode = 200) =>
        new() { StatusCode = statusCode, Status = "success", Message = message, Body = body };
}

public sealed class ApiErrorResponse
{
    public int StatusCode { get; init; }
    public string Status { get; init; } = "error";
    public string Message { get; init; } = string.Empty;
    public IEnumerable<ApiFieldError>? Errors { get; init; }
    public string? TraceId { get; init; }
}

public sealed record ApiFieldError(string Field, string Message);
