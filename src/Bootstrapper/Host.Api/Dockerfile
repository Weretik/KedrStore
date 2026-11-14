FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# === Build ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["KedrStore.sln", "./"]
COPY ["global.json", "./"]
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]

# BuildingBlocks
COPY ["src/BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "src/BuildingBlocks/BuildingBlocks.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Integrations.OneC/BuildingBlocks.Integrations.OneC.csproj", "src/BuildingBlocks/BuildingBlocks.Integrations.OneC/"]

# Catalog
COPY ["src/Modules/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Modules/Catalog/Catalog.Api/"]
COPY ["src/Modules/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Modules/Catalog/Catalog.Application/"]
COPY ["src/Modules/Catalog/Catalog.Contracts/Catalog.Contracts.csproj", "src/Modules/Catalog/Catalog.Contracts/"]
COPY ["src/Modules/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Modules/Catalog/Catalog.Domain/"]
COPY ["src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Modules/Catalog/Catalog.Infrastructure/"]

# Content
COPY ["src/Modules/Content/Content.Api/Content.Api.csproj", "src/Modules/Content/Content.Api/"]
COPY ["src/Modules/Content/Content.Application/Content.Application.csproj", "src/Modules/Content/Content.Application/"]
COPY ["src/Modules/Content/Content.Domain/Content.Domain.csproj", "src/Modules/Content/Content.Domain/"]
COPY ["src/Modules/Content/Content.Infrastructure/Content.Infrastructure.csproj", "src/Modules/Content/Content.Infrastructure/"]

# Identity
COPY ["src/Modules/Identity/Identity.Api/Identity.Api.csproj", "src/Modules/Identity/Identity.Api/"]
COPY ["src/Modules/Identity/Identity.Application/Identity.Application.csproj", "src/Modules/Identity/Identity.Application/"]
COPY ["src/Modules/Identity/Identity.Domain/Identity.Domain.csproj", "src/Modules/Identity/Identity.Domain/"]
COPY ["src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Modules/Identity/Identity.Infrastructure/"]

# Sales
COPY ["src/Modules/Sales/Sales.Api/Sales.Api.csproj", "src/Modules/Sales/Sales.Api/"]
COPY ["src/Modules/Sales/Sales.Application/Sales.Application.csproj", "src/Modules/Sales/Sales.Application/"]
COPY ["src/Modules/Sales/Sales.Domain/Sales.Domain.csproj", "src/Modules/Sales/Sales.Domain/"]
COPY ["src/Modules/Sales/Sales.Infrastructure/Sales.Infrastructure.csproj", "src/Modules/Sales/Sales.Infrastructure/"]

# Media
COPY ["src/Modules/Media/Media.Api/Media.Api.csproj", "src/Modules/Media/Media.Api/"]
COPY ["src/Modules/Media/Media.Application/Media.Application.csproj", "src/Modules/Media/Media.Application/"]
COPY ["src/Modules/Media/Media.Contracts/Media.Contracts.csproj", "src/Modules/Media/Media.Contracts/"]
COPY ["src/Modules/Media/Media.Domain/Media.Domain.csproj", "src/Modules/Media/Media.Domain/"]
COPY ["src/Modules/Media/Media.Infrastructure/Media.Infrastructure.csproj", "src/Modules/Media/Media.Infrastructure/"]

# Bootstrapper
COPY ["src/Bootstrapper/Host.Api/Host.Api.csproj", "src/Bootstrapper/Host.Api/"]

# Tests
COPY ["tests/ArchitectureTests/ArchitectureTests.csproj", "tests/ArchitectureTests/"]
COPY ["tests/IntegrationTests/IntegrationTests.csproj", "tests/IntegrationTests/"]
COPY ["tests/UnitTests/UnitTests.csproj", "tests/UnitTests/"]

COPY ["src/Modules/Identity/Identity.Contracts/Identity.Contracts.csproj", "src/Modules/Identity/Identity.Contracts/"]
COPY ["src/Modules/Work/Work.Api/Work.Api.csproj", "src/Modules/Work/Work.Api/"]
COPY ["src/Modules/Work/Work.Application/Work.Application.csproj", "src/Modules/Work/Work.Application/"]
COPY ["src/Modules/Work/Work.Domain/Work.Domain.csproj", "src/Modules/Work/Work.Domain/"]
COPY ["src/Modules/Work/Work.Infrastructure/Work.Infrastructure.csproj", "src/Modules/Work/Work.Infrastructure/"]
COPY ["src/Modules/Scheduling/Scheduling.Api/Scheduling.Api.csproj", "src/Modules/Scheduling/Scheduling.Api/"]
COPY ["src/Modules/Scheduling/Scheduling.Application/Scheduling.Application.csproj", "src/Modules/Scheduling/Scheduling.Application/"]
COPY ["src/Modules/Scheduling/Scheduling.Contracts/Scheduling.Contracts.csproj", "src/Modules/Scheduling/Scheduling.Contracts/"]
COPY ["src/Modules/Scheduling/Scheduling.Domain/Scheduling.Domain.csproj", "src/Modules/Scheduling/Scheduling.Domain/"]
COPY ["src/Modules/Scheduling/Scheduling.Infrastructure/Scheduling.Infrastructure.csproj", "src/Modules/Scheduling/Scheduling.Infrastructure/"]

RUN dotnet restore "src/Bootstrapper/Host.Api/Host.Api.csproj"

# COPY other
COPY . .
WORKDIR "/src/src/Bootstrapper/Host.Api"

# === publish ===
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Host.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# === final imag ===
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Host.Api.dll"]
