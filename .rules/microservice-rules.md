# Microservice Rules

- Keep the service focused: one responsibility — HTML to PDF.
- Do not return PDFs inside JSON. Use `FileContentResult` with `application/pdf`.
- Base64 encoding is only allowed via the explicit `/api/pdf/generate-base64` endpoint.
- Do not save temporary files unless strictly required. Keep generation in-memory.
- Do not expose Playwright internals outside of the Infrastructure layer.
- Controllers must not contain business logic. Delegate to `PdfService`.
- Do not access `IPdfGeneratorService` directly from controllers.
- Standard error format must be used for all error responses.
- Do not expose stack traces in production responses.
- Limit request body size via configuration (`MaxHtmlSizeInKb`).
