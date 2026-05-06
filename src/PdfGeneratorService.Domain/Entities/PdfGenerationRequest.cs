using PdfGeneratorService.Domain.ValueObjects;

namespace PdfGeneratorService.Domain.Entities;

public sealed class PdfGenerationRequest
{
    public string HtmlContent { get; }
    public string FileName { get; }
    public PdfFormat Format { get; }
    public bool Landscape { get; }
    public bool PrintBackground { get; }
    public PdfMargins Margins { get; }

    private PdfGenerationRequest(
        string htmlContent,
        string fileName,
        PdfFormat format,
        bool landscape,
        bool printBackground,
        PdfMargins margins)
    {
        HtmlContent = htmlContent;
        FileName = fileName;
        Format = format;
        Landscape = landscape;
        PrintBackground = printBackground;
        Margins = margins;
    }

    public static PdfGenerationRequest Create(
        string htmlContent,
        string? fileName,
        string? format,
        bool landscape,
        bool printBackground,
        PdfMargins margins)
    {
        return new PdfGenerationRequest(
            htmlContent,
            SanitizeFileName(fileName),
            PdfFormat.From(format),
            landscape,
            printBackground,
            margins);
    }

    private static string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return $"document-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));

        if (!sanitized.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            sanitized += ".pdf";

        return sanitized;
    }
}
