# Docker Rules

- Local development does NOT require Docker.
- Docker is for cloud deployments: Azure App Service Containers, Azure Container Apps, AKS, AWS ECS.
- Always use `mcr.microsoft.com/playwright/dotnet` as the runtime base image.
- Use multi-stage builds: SDK image for build, Playwright image for runtime.
- Never bake secrets or environment-specific config into the image.
- Use environment variables to configure the service at runtime.
- Expose port `8080` inside the container; map it externally as needed.
- The Playwright runtime image already includes Chromium dependencies for Linux.
- Do not install unnecessary tools or packages in the runtime image.
