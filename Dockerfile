# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PdfGeneratorService.sln .
COPY src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj                       src/PdfGeneratorService.Api/
COPY src/PdfGeneratorService.Application/PdfGeneratorService.Application.csproj       src/PdfGeneratorService.Application/
COPY src/PdfGeneratorService.Domain/PdfGeneratorService.Domain.csproj                 src/PdfGeneratorService.Domain/
COPY src/PdfGeneratorService.Infrastructure/PdfGeneratorService.Infrastructure.csproj src/PdfGeneratorService.Infrastructure/

RUN dotnet restore src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj

COPY src/ src/

RUN dotnet publish src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage (Playwright with Chromium) ──────────────────────────────────
FROM mcr.microsoft.com/playwright/dotnet:v1.59.0-noble AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

# Playwright browsers are pre-installed in the base image
# Install only chromium to reduce image size
RUN pwsh /app/playwright.ps1 install chromium --with-deps

ENTRYPOINT ["dotnet", "PdfGeneratorService.Api.dll"]
