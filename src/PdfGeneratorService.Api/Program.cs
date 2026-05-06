using PdfGeneratorService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiMiddleware();

app.Run();

// Allows integration test factory to reference this assembly
public partial class Program { }
