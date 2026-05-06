namespace PdfGeneratorService.Application;

public sealed class PdfGenerationOptions
{
    public const string SectionName = "PdfGeneration";

    public int MaxHtmlSizeInKb { get; set; } = 1024;
    public string DefaultFormat { get; set; } = "A4";
    public bool DefaultPrintBackground { get; set; } = true;
    public int DefaultTimeoutSeconds { get; set; } = 30;
}
