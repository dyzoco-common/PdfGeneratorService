namespace PdfGeneratorService.Domain.ValueObjects;

public sealed class PdfFormat
{
    public static readonly string A4 = "A4";
    public static readonly string Letter = "Letter";
    public static readonly string Legal = "Legal";

    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { "A4", "Letter", "Legal" };

    public string Value { get; }

    private PdfFormat(string value) => Value = value;

    public static PdfFormat From(string? value)
    {
        var normalized = value?.Trim() ?? A4;
        if (!Allowed.Contains(normalized))
            throw new Exceptions.DomainException(
                $"Invalid PDF format '{value}'. Allowed values: {string.Join(", ", Allowed)}.");
        return new PdfFormat(normalized.ToUpperInvariant() == "A4" ? "A4" : Capitalize(normalized));
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Allowed.Contains(value.Trim());

    private static string Capitalize(string s) =>
        char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();

    public override string ToString() => Value;
}
