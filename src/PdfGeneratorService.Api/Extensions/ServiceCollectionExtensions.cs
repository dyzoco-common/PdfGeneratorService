using PdfGeneratorService.Application;
using PdfGeneratorService.Application.DependencyInjection;
using PdfGeneratorService.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

namespace PdfGeneratorService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PdfGenerationOptions>(
            configuration.GetSection(PdfGenerationOptions.SectionName));

        services.AddApplication();
        services.AddInfrastructure();

        services.AddControllers();
        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        app.UseMiddleware<Middleware.GlobalExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "PDF Generator Service";
                options.Theme = ScalarTheme.Purple;
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
}
