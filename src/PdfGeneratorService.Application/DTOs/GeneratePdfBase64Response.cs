namespace PdfGeneratorService.Application.DTOs;

public sealed class GeneratePdfBase64Response
{
    public string FileName { get; init; } = string.Empty;
    public string ContentBase64 { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
}
