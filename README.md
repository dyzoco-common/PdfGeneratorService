# PdfGeneratorService

A .NET 10 Web API microservice that converts HTML content to PDF using **Playwright/Chromium**.

## Architecture

```
src/
  PdfGeneratorService.Api/           → Controllers, Middleware, Program.cs
  PdfGeneratorService.Application/   → PdfService, DTOs, Validators, Interfaces
  PdfGeneratorService.Domain/        → Entities, ValueObjects, Exceptions
  PdfGeneratorService.Infrastructure/→ PlaywrightPdfGeneratorService

tests/
  PdfGeneratorService.Tests/         → xUnit tests (unit + integration)
```

**Request flow:** Controller → PdfService → IPdfGeneratorService → Playwright/Chromium → PDF bytes

## Local Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PowerShell (for Playwright browser installation)
- No Docker required for local development

## Installation

```bash
git clone <repo>
cd PdfGeneratorService
dotnet restore
dotnet build
```

## Install Playwright Chromium (required once)

After building the project, run one of these depending on your PowerShell version:

**Windows (PowerShell 5 — default en Windows 11):**
```powershell
powershell -ExecutionPolicy Bypass -File "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium
```

**Windows (PowerShell 7 / pwsh instalado):**
```powershell
pwsh -ExecutionPolicy Bypass -File "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium
```

**Desde PowerShell interactivo (cualquier versión):**
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force
& "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium
```

> Los browsers se instalan en `%LOCALAPPDATA%\ms-playwright\` y no requieren Docker ni permisos de administrador.

> **Linux — dependencias adicionales:**
> ```bash
> powershell playwright.ps1 install-deps chromium
> ```

## Run Locally (without Docker)

```bash
cd src/PdfGeneratorService.Api
dotnet run
```

The API will be available at: `https://localhost:5001` / `http://localhost:5000`

## API Documentation (Scalar)

Scalar UI is available in all environments at:

```
http://localhost:5000/scalar/v1
```

OpenAPI JSON at: `http://localhost:5000/openapi/v1.json`

## Endpoints

| Method | Path                       | Description                        |
|--------|----------------------------|------------------------------------|
| POST   | `/api/pdf/generate`        | Generate PDF, returns binary file  |
| POST   | `/api/pdf/generate-base64` | Generate PDF, returns Base64 JSON  |
| GET    | `/health`                  | Health check                       |

## Example Request

```bash
curl -X POST http://localhost:5000/api/pdf/generate \
  -H "Content-Type: application/json" \
  -d '{
    "htmlContent": "<html><body><h1>Hello World</h1><p>My first PDF.</p></body></html>",
    "fileName": "hello-world",
    "format": "A4",
    "landscape": false,
    "printBackground": true,
    "marginTop": "20mm",
    "marginRight": "15mm",
    "marginBottom": "20mm",
    "marginLeft": "15mm"
  }' \
  --output hello-world.pdf
```

### Request Body

| Field            | Type    | Required | Default | Description                                          |
|------------------|---------|----------|---------|------------------------------------------------------|
| `htmlContent`    | string  | Yes      | —       | Full HTML string                                     |
| `fileName`       | string  | No       | auto    | Output file name (sanitized)                         |
| `format`         | string  | No       | A4      | `A4`, `Letter`, `Legal`. Mutually exclusive with `width` |
| `width`          | string  | No       | —       | Custom paper width. e.g. `80mm`, `8cm`. Mutually exclusive with `format` |
| `height`         | string  | No*      | —       | Custom paper height. e.g. `150mm`. Required when `width` is set |
| `landscape`      | bool    | No       | false   | Landscape orientation                                |
| `printBackground`| bool    | No       | true    | Include CSS backgrounds                              |
| `marginTop`      | string  | No       | null    | e.g. `20mm`, `1cm`, `15px`                          |
| `marginRight`    | string  | No       | null    |                                                      |
| `marginBottom`   | string  | No       | null    |                                                      |
| `marginLeft`     | string  | No       | null    |                                                      |

> **`format` vs `width`/`height`:** Use `format` for standard paper sizes (A4, Letter, Legal). Use `width` + `height` for custom dimensions such as POS receipt printers. Both cannot be sent in the same request.

### POS / Custom Paper Size Example

For thermal receipt printers (80mm roll):

```bash
curl -X POST http://localhost:5000/api/pdf/generate \
  -H "Content-Type: application/json" \
  -d '{
    "htmlContent": "<html><body><p>Receipt content here</p></body></html>",
    "fileName": "receipt",
    "width": "80mm",
    "height": "150mm",
    "printBackground": true,
    "marginTop": "5mm",
    "marginRight": "5mm",
    "marginBottom": "5mm",
    "marginLeft": "5mm"
  }' \
  --output receipt.pdf
```

Common widths: `80mm` (standard roll), `58mm` (narrow roll). Set `height` to match the expected receipt length, or use a generous value — content will not overflow beyond it.

### Page Breaks

Page breaks are controlled entirely from the HTML/CSS inside `htmlContent`. No additional API parameters are needed — Playwright renders the page in print mode and honours standard CSS paging properties.

**Force a page break before a section:**

```html
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<style>
  .nueva-pagina {
    break-before: page;        /* modern standard */
    page-break-before: always; /* fallback */
  }
</style>
</head>
<body>

  <!-- Page 1: partial content, does not need to fill the sheet -->
  <div>
    <h1>Invoice Header</h1>
    <p>Client: Juan Pérez</p>
    <p>Date: 2026-05-07</p>
  </div>

  <!-- Page 2: starts on a new sheet regardless of how much space was left -->
  <div class="nueva-pagina">
    <h2>Product Detail</h2>
    <table>...</table>
  </div>

</body>
</html>
```

**Prevent a block from splitting across pages:**

```css
.no-split {
  break-inside: avoid;
}
```

Useful for table rows, cards, or any block that should never be cut mid-page.

**Repeat a header on every page:**

```css
.page-header {
  position: fixed;
  top: 0;
  width: 100%;
}
```

**CSS reference:**

| Property | Effect |
|---|---|
| `break-before: page` | Force a new page before the element |
| `break-after: page` | Force a new page after the element |
| `break-inside: avoid` | Prevent the element from splitting across pages |
| `position: fixed; top: 0` | Repeat content on every page (headers/footers) |

> **Important:** Always send a complete HTML document with `<!DOCTYPE html>`, `<html>`, `<head>`, and `<body>`. Chromium may ignore CSS paging rules on partial/malformed HTML. Apply `break-before: page` on the element that *starts* a new page — not `break-after` on an empty separator element, which Chromium collapses and ignores.

### Response

- `200 OK` — `Content-Type: application/pdf` with binary PDF data
- `400 Bad Request` — Validation error (JSON)

### Error Response Format

```json
{
  "statusCode": 400,
  "status": "error",
  "message": "Validation failed.",
  "errors": [
    { "field": "HtmlContent", "message": "htmlContent is required." }
  ],
  "traceId": "00-abc123-def456-00"
}
```

## Run Tests

```bash
dotnet test
```

With coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Configuration

`appsettings.json`:

```json
{
  "PdfGeneration": {
    "MaxHtmlSizeInKb": 1024,
    "DefaultFormat": "A4",
    "DefaultPrintBackground": true,
    "DefaultTimeoutSeconds": 30
  }
}
```

## Build Docker Image

```bash
docker build -t pdf-generator-service:latest .
docker run -p 8080:8080 pdf-generator-service:latest
```

## Deploy to Azure App Service for Containers

```bash
az acr build --registry <your-registry> --image pdf-generator-service:latest .
az webapp create \
  --name pdf-generator-service \
  --resource-group <rg> \
  --plan <plan> \
  --deployment-container-image-name <your-registry>.azurecr.io/pdf-generator-service:latest
```

## Deploy to Kubernetes

```bash
kubectl apply -f k8s/deployment.yml
kubectl apply -f k8s/service.yml
kubectl apply -f k8s/ingress.yml
```

Update the image in `k8s/deployment.yml` before applying.

## Troubleshooting

### Chromium not installed

```
Microsoft.Playwright.PlaywrightException: Executable doesn't exist at
  C:\Users\...\ms-playwright\chromium_headless_shell-XXXX\...
```

Este error ocurre cuando los browsers de Playwright no están instalados localmente.
Ejecuta el script de instalación **después de compilar el proyecto**:

```powershell
# Windows PowerShell 5 (default en Windows 11)
powershell -ExecutionPolicy Bypass -File "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium

# PowerShell 7
pwsh -ExecutionPolicy Bypass -File "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium
```

Los browsers se descargan a `%LOCALAPPDATA%\ms-playwright\` y no requieren permisos de administrador.

### Error: script execution disabled (Windows)

```
playwright.ps1 cannot be loaded because running scripts is disabled on this system.
```

Solución — ejecuta desde PowerShell interactivo:
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force
& "src\PdfGeneratorService.Api\bin\Debug\net10.0\playwright.ps1" install chromium
```

### Missing Linux dependencies (CI/CD or Docker)

```bash
pwsh playwright.ps1 install-deps chromium
```

### Font rendering issues on Linux

Install fonts:
```bash
apt-get install -y fonts-liberation fonts-noto
```

### Docker: Chromium sandbox error

The Dockerfile already includes `--no-sandbox` and `--disable-setuid-sandbox` flags. If you see sandbox errors when building a custom image, ensure those args are passed.

### Request too large

Increase `MaxHtmlSizeInKb` in `appsettings.json` or via environment variable:
```
PdfGeneration__MaxHtmlSizeInKb=2048
```

## Security Notes

- **HTML from untrusted users:** Playwright executes the HTML in a real browser. If accepting HTML from external users, sanitize it first (e.g., with HtmlSanitizer) to prevent SSRF, local file access, or credential theft via CSS/JS.
- HTML content is never logged.
- Stack traces are only exposed in Development environment.
