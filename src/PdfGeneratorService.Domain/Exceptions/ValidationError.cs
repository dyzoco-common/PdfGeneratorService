namespace PdfGeneratorService.Domain.Exceptions;

public sealed record ValidationError(string Field, string Message);
