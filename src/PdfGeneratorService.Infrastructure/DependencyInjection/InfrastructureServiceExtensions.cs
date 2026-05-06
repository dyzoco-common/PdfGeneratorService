using Microsoft.Extensions.DependencyInjection;
using PdfGeneratorService.Application.Interfaces;
using PdfGeneratorService.Infrastructure.Pdf;

namespace PdfGeneratorService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPdfGeneratorService, PlaywrightPdfGeneratorService>();
        return services;
    }
}
