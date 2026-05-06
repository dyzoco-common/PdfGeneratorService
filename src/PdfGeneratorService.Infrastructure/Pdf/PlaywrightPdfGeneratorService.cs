using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using PdfGeneratorService.Application;
using PdfGeneratorService.Application.Interfaces;
using PdfGeneratorService.Domain.Entities;

namespace PdfGeneratorService.Infrastructure.Pdf;

/// <summary>
/// Generates PDFs using Playwright/Chromium.
/// Requires browsers installed locally:
///   dotnet tool install --global Microsoft.Playwright.CLI
///   playwright install chromium
/// Or after build:
///   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
/// </summary>
public sealed class PlaywrightPdfGeneratorService : IPdfGeneratorService, IAsyncDisposable
{
    private readonly PdfGenerationOptions _options;
    private readonly ILogger<PlaywrightPdfGeneratorService> _logger;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public PlaywrightPdfGeneratorService(
        IOptions<PdfGenerationOptions> options,
        ILogger<PlaywrightPdfGeneratorService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(PdfGenerationRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        IPage? page = null;
        try
        {
            page = await _browser!.NewPageAsync();

            await page.SetContentAsync(request.HtmlContent, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = _options.DefaultTimeoutSeconds * 1000
            });

            var pdfOptions = BuildPdfOptions(request);
            var bytes = await page.PdfAsync(pdfOptions);

            return bytes;
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright error during PDF generation. Format={Format}", request.Format);
            throw new InvalidOperationException("PDF generation failed due to a Playwright error.", ex);
        }
        finally
        {
            if (page is not null)
                await page.CloseAsync();
        }
    }

    private PagePdfOptions BuildPdfOptions(PdfGenerationRequest request)
    {
        var options = new PagePdfOptions
        {
            Format = request.Format.Value,
            Landscape = request.Landscape,
            PrintBackground = request.PrintBackground,
        };

        var m = request.Margins;
        if (m.Top is not null || m.Right is not null || m.Bottom is not null || m.Left is not null)
        {
            options.Margin = new Margin
            {
                Top = m.Top,
                Right = m.Right,
                Bottom = m.Bottom,
                Left = m.Left
            };
        }

        return options;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing Playwright browser instance.");
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
            });
            _initialized = true;
            _logger.LogInformation("Playwright browser initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialize Playwright. Ensure Chromium is installed. " +
                "Run: pwsh bin/Debug/net10.0/playwright.ps1 install chromium");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
        _initLock.Dispose();
    }
}
