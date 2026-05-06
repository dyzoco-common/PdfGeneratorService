using FluentValidation;
using PdfGeneratorService.Application.DTOs;
using PdfGeneratorService.Domain.ValueObjects;

namespace PdfGeneratorService.Application.Validators;

public sealed class GeneratePdfRequestValidator : AbstractValidator<GeneratePdfRequest>
{
    private static readonly string[] AllowedMarginUnits = ["px", "cm", "mm", "in", "%"];

    public GeneratePdfRequestValidator()
    {
        RuleFor(x => x.HtmlContent)
            .NotNull().WithMessage("htmlContent is required.")
            .NotEmpty().WithMessage("htmlContent cannot be empty.");

        RuleFor(x => x.Format)
            .Must(f => f == null || PdfFormat.IsValid(f))
            .WithMessage("format must be one of: A4, Letter, Legal.");

        RuleFor(x => x.MarginTop)
            .Must(m => m == null || IsValidMargin(m))
            .WithMessage("marginTop must be a valid CSS margin value (e.g. '10mm', '1cm', '20px').");

        RuleFor(x => x.MarginRight)
            .Must(m => m == null || IsValidMargin(m))
            .WithMessage("marginRight must be a valid CSS margin value.");

        RuleFor(x => x.MarginBottom)
            .Must(m => m == null || IsValidMargin(m))
            .WithMessage("marginBottom must be a valid CSS margin value.");

        RuleFor(x => x.MarginLeft)
            .Must(m => m == null || IsValidMargin(m))
            .WithMessage("marginLeft must be a valid CSS margin value.");
    }

    private static bool IsValidMargin(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        foreach (var unit in AllowedMarginUnits)
        {
            if (value.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                var number = value[..^unit.Length];
                return double.TryParse(number, out var d) && d >= 0;
            }
        }
        return value == "0";
    }
}
