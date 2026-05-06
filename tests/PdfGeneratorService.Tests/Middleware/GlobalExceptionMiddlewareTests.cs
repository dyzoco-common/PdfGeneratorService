using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PdfGeneratorService.Api.Middleware;
using PdfGeneratorService.Domain.Exceptions;
using System.Text.Json;
using ValidationException = PdfGeneratorService.Domain.Exceptions.ValidationException;

namespace PdfGeneratorService.Tests.Middleware;

public sealed class GlobalExceptionMiddlewareTests
{
    private readonly Mock<IHostEnvironment> _envMock = new();

    public GlobalExceptionMiddlewareTests()
    {
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
    }

    [Fact]
    public async Task Should_Return_400_For_ValidationException()
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ValidationException("htmlContent", "htmlContent is required."),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            _envMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_422_For_DomainException()
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new DomainException("Domain rule violated."),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            _envMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(422, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_500_For_UnhandledException()
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new Exception("Unexpected failure."),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            _envMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Json_With_Status_Error()
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ValidationException("field", "Error message."),
            NullLogger<GlobalExceptionMiddleware>.Instance,
            _envMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("error", doc.RootElement.GetProperty("status").GetString());
    }
}
