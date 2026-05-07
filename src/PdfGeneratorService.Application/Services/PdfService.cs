using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfGeneratorService.Application.DTOs;
using PdfGeneratorService.Application.Interfaces;
using PdfGeneratorService.Domain.Entities;
using PdfGeneratorService.Domain.Exceptions;
using PdfGeneratorService.Domain.ValueObjects;
using System.Diagnostics;
using ValidationException = PdfGeneratorService.Domain.Exceptions.ValidationException;

namespace PdfGeneratorService.Application.Services;

public sealed class PdfService
{
    private readonly IPdfGeneratorService _generator;
    private readonly IValidator<GeneratePdfRequest> _validator;
    private readonly PdfGenerationOptions _options;
    private readonly ILogger<PdfService> _logger;

    public PdfService(
        IPdfGeneratorService generator,
        IValidator<GeneratePdfRequest> validator,
        IOptions<PdfGenerationOptions> options,
        ILogger<PdfService> logger)
    {
        _generator = generator;
        _validator = validator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(byte[] Bytes, string FileName)> GeneratePdfAsync(
        GeneratePdfRequest dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating PDF generation request. FileName={FileName}", dto.FileName);

        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage));
            throw new ValidationException("Validation failed.", errors);
        }

        var htmlSizeKb = (dto.HtmlContent.Length * sizeof(char)) / 1024;
        if (htmlSizeKb > _options.MaxHtmlSizeInKb)
            throw new ValidationException("htmlContent",
                $"htmlContent exceeds the maximum allowed size of {_options.MaxHtmlSizeInKb} KB.");

        var formatOrDefault = dto.Width is null ? (dto.Format ?? _options.DefaultFormat) : null;
        var request = PdfGenerationRequest.Create(
            dto.HtmlContent,
            dto.FileName,
            formatOrDefault,
            dto.Width,
            dto.Height,
            dto.Landscape ?? false,
            dto.PrintBackground ?? _options.DefaultPrintBackground,
            new PdfMargins(dto.MarginTop, dto.MarginRight, dto.MarginBottom, dto.MarginLeft));

        _logger.LogInformation(
            "Starting PDF generation. Format={Format}, Width={Width}, Height={Height}, Landscape={Landscape}, PrintBackground={PrintBackground}",
            request.Format, request.Width, request.Height, request.Landscape, request.PrintBackground);

        var sw = Stopwatch.StartNew();
        var bytes = await _generator.GenerateAsync(request, cancellationToken);
        sw.Stop();

        _logger.LogInformation(
            "PDF generated successfully. FileName={FileName}, SizeKb={SizeKb}, ElapsedMs={ElapsedMs}",
            request.FileName, bytes.Length / 1024, sw.ElapsedMilliseconds);

        return (bytes, request.FileName);
    }
}
