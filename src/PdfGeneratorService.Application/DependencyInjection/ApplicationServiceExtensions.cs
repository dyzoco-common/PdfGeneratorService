using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PdfGeneratorService.Application.Services;
using PdfGeneratorService.Application.Validators;

namespace PdfGeneratorService.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PdfService>();
        services.AddValidatorsFromAssemblyContaining<GeneratePdfRequestValidator>();
        return services;
    }
}
