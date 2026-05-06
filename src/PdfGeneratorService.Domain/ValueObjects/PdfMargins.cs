namespace PdfGeneratorService.Domain.ValueObjects;

public sealed record PdfMargins(
    string? Top,
    string? Right,
    string? Bottom,
    string? Left);
