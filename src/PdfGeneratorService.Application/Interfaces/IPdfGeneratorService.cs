using PdfGeneratorService.Domain.Entities;

namespace PdfGeneratorService.Application.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateAsync(PdfGenerationRequest request, CancellationToken cancellationToken = default);
}
