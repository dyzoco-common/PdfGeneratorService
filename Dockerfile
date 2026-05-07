# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PdfGeneratorService.slnx .
COPY src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj                       src/PdfGeneratorService.Api/
COPY src/PdfGeneratorService.Application/PdfGeneratorService.Application.csproj       src/PdfGeneratorService.Application/
COPY src/PdfGeneratorService.Domain/PdfGeneratorService.Domain.csproj                 src/PdfGeneratorService.Domain/
COPY src/PdfGeneratorService.Infrastructure/PdfGeneratorService.Infrastructure.csproj src/PdfGeneratorService.Infrastructure/

RUN dotnet restore src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj

COPY src/ src/

RUN dotnet publish src/PdfGeneratorService.Api/PdfGeneratorService.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage (.NET 10 + Chromium via Playwright) ─────────────────────────
  FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
  WORKDIR /app

  # Install PowerShell (required by playwright.ps1) and Chromium with its system dependencies
  RUN apt-get update && \
      apt-get install -y wget && \
      wget -q "https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb" -O /tmp/ms-prod.deb && \
      dpkg -i /tmp/ms-prod.deb && \
      rm /tmp/ms-prod.deb && \
      apt-get update && \
      apt-get install -y powershell && \
      rm -rf /var/lib/apt/lists/*

  ENV ASPNETCORE_URLS=http://+:8080
  ENV ASPNETCORE_ENVIRONMENT=Production
  EXPOSE 8080

  COPY --from=build /app/publish .

  RUN pwsh /app/playwright.ps1 install chromium --with-deps

  ENTRYPOINT ["dotnet", "PdfGeneratorService.Api.dll"]
