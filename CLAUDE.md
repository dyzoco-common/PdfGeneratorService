# CLAUDE.md — PdfGeneratorService

## Context

This is a .NET 10 Web API microservice that receives HTML content and returns a generated PDF using Playwright/Chromium. It follows a lightweight DDD structure with clear separation of concerns.

## Objective

Convert HTML templates to PDF documents via a REST API. The service is designed to be deployed in cloud container environments (Azure Container Apps, AKS, AWS ECS) but runs locally without Docker.

## Architecture Rules

- **Domain** — Entities, Value Objects, Exceptions. No framework dependencies.
- **Application** — Interfaces, DTOs, Validators, PdfService. Orchestrates use cases.
- **Infrastructure** — Playwright implementation of `IPdfGeneratorService`. No business logic.
- **Api** — Controllers, Middleware, Extensions, Program.cs. No business logic.

Controllers call `PdfService`. `PdfService` calls `IPdfGeneratorService`. `PlaywrightPdfGeneratorService` implements `IPdfGeneratorService`.

Do not bypass this flow. Do not call `IPdfGeneratorService` from controllers.

## Code Style Rules

- `PascalCase` for C# identifiers.
- `camelCase` in JSON (enforced via `JsonNamingPolicy.CamelCase`).
- `sealed` classes by default unless inheritance is needed.
- `nullable enable` always.
- `async/await` everywhere, no `.Result` or `.Wait()`.
- No comments unless the WHY is non-obvious.
- No multi-line docstrings. XML doc on public API only.

## Testing Rules

- xUnit + Moq + `Microsoft.AspNetCore.Mvc.Testing`.
- Test names: `Should_[ExpectedBehavior]_When_[Condition]`.
- Mock `IPdfGeneratorService` in all unit and integration tests.
- Test behaviour, not implementation.
- Do not test trivial code (getters/setters).

## Logging Rules

- Log at INFO: PDF generation start, format, elapsed time, output size.
- Log at WARN: validation errors.
- Log at ERROR: Playwright failures, unexpected exceptions (with context).
- Never log the full HTML content received (security risk).
- Never log sensitive headers or request metadata unnecessarily.

## Error Handling Rules

- All unhandled exceptions are caught by `GlobalExceptionMiddleware`.
- `ValidationException` → HTTP 400.
- `DomainException` → HTTP 422.
- Unexpected exceptions → HTTP 500.
- Stack traces are only exposed in Development environment.
- Error response format is always:
  ```json
  { "statusCode": 400, "status": "error", "message": "...", "errors": [...], "traceId": "..." }
  ```

## Overengineering Rules

Do NOT introduce:
- MediatR or CQRS patterns
- Repository pattern (no database)
- AutoMapper
- Event bus or messaging
- Feature flags
- Multiple PDF providers/strategy pattern (unless explicitly requested)

If a new feature is needed, add it directly in the relevant layer following the existing patterns.

## Playwright Notes

- `PlaywrightPdfGeneratorService` is registered as a `Singleton` and manages the browser lifecycle.
- The browser is initialized lazily on first request, with a `SemaphoreSlim` lock.
- Browsers must be installed locally before running:
  ```
  pwsh bin/Debug/net10.0/playwright.ps1 install chromium
  ```
- In Docker, the Playwright base image already includes Chromium.

## Documentation Rules

After completing any change that affects the public API, deployment, or behavior visible to consumers, update `README.md` before considering the task done. Changes that always require a README update:

- New or modified request/response fields
- New endpoints
- New environment variables or configuration keys
- Changes to deployment steps (Docker, Azure, Kubernetes)
- Changes to Scalar/OpenAPI availability
- New troubleshooting scenarios

## Future Modifications

When adding new PDF options (e.g., headers/footers, page ranges):
1. Add optional fields to `GeneratePdfRequest` DTO.
2. Add validation in `GeneratePdfRequestValidator`.
3. Add to `PdfGenerationRequest` entity if it's a domain concept.
4. Pass through `PdfService` → `PlaywrightPdfGeneratorService`.
5. Map to `PagePdfOptions` in `BuildPdfOptions`.
6. Update `README.md` request body table and add an example if needed.
