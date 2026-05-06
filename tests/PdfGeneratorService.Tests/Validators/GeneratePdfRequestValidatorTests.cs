using PdfGeneratorService.Application.DTOs;
using PdfGeneratorService.Application.Validators;

namespace PdfGeneratorService.Tests.Validators;

public sealed class GeneratePdfRequestValidatorTests
{
    private readonly GeneratePdfRequestValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_HtmlContent_Is_Null()
    {
        var request = new GeneratePdfRequest { HtmlContent = null! };
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HtmlContent");
    }

    [Fact]
    public async Task Should_Fail_When_HtmlContent_Is_Empty()
    {
        var request = new GeneratePdfRequest { HtmlContent = "" };
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HtmlContent");
    }

    [Fact]
    public async Task Should_Pass_With_Valid_HtmlContent()
    {
        var request = new GeneratePdfRequest { HtmlContent = "<html><body>Test</body></html>" };
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("A4")]
    [InlineData("Letter")]
    [InlineData("Legal")]
    [InlineData(null)]
    public async Task Should_Pass_With_Valid_Format(string? format)
    {
        var request = new GeneratePdfRequest { HtmlContent = "<p>test</p>", Format = format };
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("A3")]
    [InlineData("invalid")]
    [InlineData("Tabloid")]
    public async Task Should_Fail_With_Invalid_Format(string format)
    {
        var request = new GeneratePdfRequest { HtmlContent = "<p>test</p>", Format = format };
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Format");
    }

    [Theory]
    [InlineData("10mm")]
    [InlineData("1cm")]
    [InlineData("20px")]
    [InlineData("1in")]
    [InlineData("0")]
    public async Task Should_Pass_With_Valid_Margin(string margin)
    {
        var request = new GeneratePdfRequest
        {
            HtmlContent = "<p>test</p>",
            MarginTop = margin
        };
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-10mm")]
    [InlineData("10")]
    public async Task Should_Fail_With_Invalid_Margin(string margin)
    {
        var request = new GeneratePdfRequest
        {
            HtmlContent = "<p>test</p>",
            MarginTop = margin
        };
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MarginTop");
    }
}
