namespace PdfGeneratorService.Application.DTOs;

public sealed class GeneratePdfRequest
{
    public string HtmlContent { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? Format { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public bool? Landscape { get; set; }
    public bool? PrintBackground { get; set; }
    public string? MarginTop { get; set; }
    public string? MarginRight { get; set; }
    public string? MarginBottom { get; set; }
    public string? MarginLeft { get; set; }
}
