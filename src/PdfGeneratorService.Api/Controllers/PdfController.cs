using Microsoft.AspNetCore.Mvc;
using PdfGeneratorService.Application.DTOs;
using PdfGeneratorService.Application.Responses;
using PdfGeneratorService.Application.Services;

namespace PdfGeneratorService.Api.Controllers;

[ApiController]
[Route("api/pdf")]
[Produces("application/json")]
public sealed class PdfController : ControllerBase
{
    private readonly PdfService _pdfService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(PdfService pdfService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>Generates a PDF from HTML content and returns the binary file.</summary>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/pdf/generate
    ///     {
    ///       "htmlContent": "&lt;html&gt;&lt;body&gt;&lt;h1&gt;Hello World&lt;/h1&gt;&lt;/body&gt;&lt;/html&gt;",
    ///       "fileName": "my-report",
    ///       "format": "A4",
    ///       "landscape": false,
    ///       "printBackground": true,
    ///       "marginTop": "20mm",
    ///       "marginRight": "15mm",
    ///       "marginBottom": "20mm",
    ///       "marginLeft": "15mm"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the generated PDF file</response>
    /// <response code="400">Validation error in the request</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPost("generate")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate(
        [FromBody] GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        var (bytes, fileName) = await _pdfService.GeneratePdfAsync(request, cancellationToken);
        return File(bytes, "application/pdf", fileName);
    }

    /// <summary>Generates a PDF from HTML content and returns it as Base64.</summary>
    /// <response code="200">Returns a JSON object with the PDF encoded as Base64</response>
    /// <response code="400">Validation error in the request</response>
    [HttpPost("generate-base64")]
    [ProducesResponseType(typeof(ApiResponse<GeneratePdfBase64Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateBase64(
        [FromBody] GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        var (bytes, fileName) = await _pdfService.GeneratePdfAsync(request, cancellationToken);

        var result = new GeneratePdfBase64Response
        {
            FileName = fileName,
            ContentBase64 = Convert.ToBase64String(bytes),
            SizeInBytes = bytes.Length
        };

        return Ok(ApiResponse<GeneratePdfBase64Response>.Success(result, "PDF generated successfully."));
    }
}
