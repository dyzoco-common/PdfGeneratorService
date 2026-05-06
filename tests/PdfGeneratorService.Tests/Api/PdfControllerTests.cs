using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PdfGeneratorService.Application.Interfaces;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PdfGeneratorService.Tests.Api;

public sealed class PdfControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PdfControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Generate_Should_Return_400_When_HtmlContent_Is_Missing()
    {
        var client = _factory.CreateClient();
        var body = new { fileName = "test" };

        var response = await client.PostAsJsonAsync("/api/pdf/generate", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Generate_Should_Return_400_When_HtmlContent_Is_Empty()
    {
        var client = CreateClientWithMockedGenerator(Array.Empty<byte>());
        var body = new { htmlContent = "" };

        var response = await client.PostAsJsonAsync("/api/pdf/generate", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("error", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Generate_Should_Return_Pdf_Content_Type_On_Success()
    {
        var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var client = CreateClientWithMockedGenerator(fakePdfBytes);

        var body = new { htmlContent = "<html><body><h1>Test</h1></body></html>", fileName = "test" };
        var response = await client.PostAsJsonAsync("/api/pdf/generate", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HealthCheck_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClientWithMockedGenerator(byte[] returnBytes)
    {
        var mock = new Mock<IPdfGeneratorService>();
        mock.Setup(s => s.GenerateAsync(It.IsAny<PdfGeneratorService.Domain.Entities.PdfGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnBytes);

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPdfGeneratorService));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddSingleton(mock.Object);
            });
        }).CreateClient();
    }
}
