# === Runtime образ ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# В .NET 8+ стандартный порт 8080 (non-root)
EXPOSE 8080
EXPOSE 8081

# === Build ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["KedrStore.sln", "./"]

# BuildingBlocks
COPY ["src/BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj", "src/BuildingBlocks/BuildingBlocks.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Integrations.OneC/BuildingBlocks.Integrations.OneC.csproj", "src/BuildingBlocks/BuildingBlocks.Integrations.OneC/"]

# Catalog
COPY ["src/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Catalog/Catalog.Api/"]
COPY ["src/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Catalog/Catalog.Application/"]
COPY ["src/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Catalog/Catalog.Domain/"]
COPY ["src/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Catalog/Catalog.Infrastructure/"]

# Identity
COPY ["src/Identity/Identity.Application/Identity.Application.csproj", "src/Identity/Identity.Application/"]
COPY ["src/Identity/Identity.Domain/Identity.Domain.csproj", "src/Identity/Identity.Domain/"]
COPY ["src/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Identity/Identity.Infrastructure/"]

# Bootstrapper
COPY ["src/Bootstrapper/Host.Api/Host.Api.csproj", "src/Bootstrapper/Host.Api/"]

# Tests
COPY ["tests/ArchitectureTests/ArchitectureTests.csproj", "tests/ArchitectureTests/"]
COPY ["tests/IntegrationTests/IntegrationTests.csproj", "tests/IntegrationTests/"]
COPY ["tests/UnitTests/UnitTests.csproj", "tests/UnitTests/"]

RUN dotnet restore "KedrStore.sln"

# COPY other
COPY . .
WORKDIR "/src/src/Bootstrapper/Host.Api"
RUN dotnet build "Host.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# === publish ===
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Host.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# === final imag ===
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Instal wget for healthcheck
USER root
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

RUN useradd -m kedruser && chown -R kedruser:kedruser /app
USER kedruser

ENTRYPOINT ["dotnet", "Host.Api.dll"]
