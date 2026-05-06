using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PdfGeneratorService.Application;
using PdfGeneratorService.Application.DTOs;
using PdfGeneratorService.Application.Interfaces;
using PdfGeneratorService.Application.Services;
using PdfGeneratorService.Application.Validators;
using PdfGeneratorService.Domain.Exceptions;
using ValidationException = PdfGeneratorService.Domain.Exceptions.ValidationException;

namespace PdfGeneratorService.Tests.Application;

public sealed class PdfServiceTests
{
    private readonly Mock<IPdfGeneratorService> _generatorMock = new();
    private readonly PdfService _service;

    public PdfServiceTests()
    {
        var options = Options.Create(new PdfGenerationOptions
        {
            MaxHtmlSizeInKb = 1024,
            DefaultFormat = "A4",
            DefaultPrintBackground = true,
            DefaultTimeoutSeconds = 30
        });

        _service = new PdfService(
            _generatorMock.Object,
            new GeneratePdfRequestValidator(),
            options,
            NullLogger<PdfService>.Instance);
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_HtmlContent_Is_Empty()
    {
        var request = new GeneratePdfRequest { HtmlContent = "" };
        await Assert.ThrowsAsync<ValidationException>(() => _service.GeneratePdfAsync(request));
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_Format_Is_Invalid()
    {
        var request = new GeneratePdfRequest { HtmlContent = "<p>test</p>", Format = "A3" };
        await Assert.ThrowsAsync<ValidationException>(() => _service.GeneratePdfAsync(request));
    }

    [Fact]
    public async Task Should_Return_PdfBytes_When_Request_Is_Valid()
    {
        var expectedBytes = "<PDF>"u8.ToArray();
        _generatorMock
            .Setup(g => g.GenerateAsync(It.IsAny<PdfGeneratorService.Domain.Entities.PdfGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBytes);

        var request = new GeneratePdfRequest
        {
            HtmlContent = "<html><body><h1>Test</h1></body></html>",
            FileName = "test",
            Format = "A4"
        };

        var (bytes, fileName) = await _service.GeneratePdfAsync(request);

        Assert.Equal(expectedBytes, bytes);
        Assert.Equal("test.pdf", fileName);
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_HtmlContent_Exceeds_Max_Size()
    {
        var options = Options.Create(new PdfGenerationOptions { MaxHtmlSizeInKb = 1 });
        var service = new PdfService(
            _generatorMock.Object,
            new GeneratePdfRequestValidator(),
            options,
            NullLogger<PdfService>.Instance);

        var largeHtml = new string('x', 2000);
        var request = new GeneratePdfRequest { HtmlContent = largeHtml };

        await Assert.ThrowsAsync<ValidationException>(() => service.GeneratePdfAsync(request));
    }
}
