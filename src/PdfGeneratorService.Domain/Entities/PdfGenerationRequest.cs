using PdfGeneratorService.Domain.ValueObjects;

namespace PdfGeneratorService.Domain.Entities;

public sealed class PdfGenerationRequest
{
    public string HtmlContent { get; }
    public string FileName { get; }
    public PdfFormat? Format { get; }
    public string? Width { get; }
    public string? Height { get; }
    public bool Landscape { get; }
    public bool PrintBackground { get; }
    public PdfMargins Margins { get; }

    private PdfGenerationRequest(
        string htmlContent,
        string fileName,
        PdfFormat? format,
        string? width,
        string? height,
        bool landscape,
        bool printBackground,
        PdfMargins margins)
    {
        HtmlContent = htmlContent;
        FileName = fileName;
        Format = format;
        Width = width;
        Height = height;
        Landscape = landscape;
        PrintBackground = printBackground;
        Margins = margins;
    }

    public static PdfGenerationRequest Create(
        string htmlContent,
        string? fileName,
        string? format,
        string? width,
        string? height,
        bool landscape,
        bool printBackground,
        PdfMargins margins)
    {
        var pdfFormat = width is null ? PdfFormat.From(format) : null;

        return new PdfGenerationRequest(
            htmlContent,
            SanitizeFileName(fileName),
            pdfFormat,
            width,
            height,
            landscape,
            printBackground,
            margins);
    }

    // Fixed cross-platform set — uses Windows-invalid chars as the most restrictive baseline.
    // Path.GetInvalidFileNameChars() omits ':' on Linux, which would produce filenames
    // invalid on Windows and inconsistent across environments.
    private static readonly HashSet<char> InvalidFileNameChars =
        [.. Path.GetInvalidFileNameChars(), ':', '*', '?', '"', '<', '>', '|', '\\'];

    private static string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return $"document-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        var sanitized = string.Concat(fileName.Select(c => InvalidFileNameChars.Contains(c) ? '_' : c));

        if (!sanitized.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            sanitized += ".pdf";

        return sanitized;
    }
}
