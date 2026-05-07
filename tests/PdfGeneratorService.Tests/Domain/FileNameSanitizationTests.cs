using PdfGeneratorService.Domain.Entities;
using PdfGeneratorService.Domain.ValueObjects;

namespace PdfGeneratorService.Tests.Domain;

public sealed class FileNameSanitizationTests
{
    [Fact]
    public void Should_Add_Pdf_Extension_When_Missing()
    {
        var request = PdfGenerationRequest.Create("<p/>", "myfile", null, null, null, false, true,
            new PdfMargins(null, null, null, null));
        Assert.EndsWith(".pdf", request.FileName);
    }

    [Fact]
    public void Should_Replace_Invalid_Characters()
    {
        var request = PdfGenerationRequest.Create("<p/>", "my:file/name?", null, null, null, false, true,
            new PdfMargins(null, null, null, null));
        Assert.DoesNotContain(":", request.FileName);
        Assert.DoesNotContain("/", request.FileName);
        Assert.DoesNotContain("?", request.FileName);
    }

    [Fact]
    public void Should_Generate_Default_FileName_When_Null()
    {
        var request = PdfGenerationRequest.Create("<p/>", null, null, null, null, false, true,
            new PdfMargins(null, null, null, null));
        Assert.StartsWith("document-", request.FileName);
        Assert.EndsWith(".pdf", request.FileName);
    }

    [Fact]
    public void Should_Keep_Valid_FileName_Unchanged()
    {
        var request = PdfGenerationRequest.Create("<p/>", "report-2024.pdf", null, null, null, false, true,
            new PdfMargins(null, null, null, null));
        Assert.Equal("report-2024.pdf", request.FileName);
    }
}
