FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["KedrStore.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj", "src/Bootstrapper/Host.Jobs/Host.Jobs/"]
COPY ["src/Catalog/Catalog.Application/Catalog.Application.csproj", "src/Catalog/Catalog.Application/"]
COPY ["src/Catalog/Catalog.Contracts/Catalog.Contracts.csproj", "src/Catalog/Catalog.Contracts/"]
COPY ["src/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Catalog/Catalog.Infrastructure/"]
COPY ["src/Catalog/Catalog.Domain/Catalog.Domain.csproj", "src/Catalog/Catalog.Domain/"]
COPY ["src/Catalog/Catalog.Api/Catalog.Api.csproj", "src/Catalog/Catalog.Api/"]
COPY ["src/Content/Content.Infrastructure/Content.Infrastructure.csproj", "src/Content/Content.Infrastructure/"]
COPY ["src/Content/Content.Application/Content.Application.csproj", "src/Content/Content.Application/"]
COPY ["src/Content/Content.Domain/Content.Domain.csproj", "src/Content/Content.Domain/"]
COPY ["src/Media/Media.Infrastructure/Media.Infrastructure.csproj", "src/Media/Media.Infrastructure/"]
COPY ["src/Media/Media.Application/Media.Application.csproj", "src/Media/Media.Application/"]
COPY ["src/Media/Media.Contracts/Media.Contracts.csproj", "src/Media/Media.Contracts/"]
COPY ["src/Media/Media.Domain/Media.Domain.csproj", "src/Media/Media.Domain/"]
COPY ["src/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "src/Identity/Identity.Infrastructure/"]
COPY ["src/Identity/Identity.Application/Identity.Application.csproj", "src/Identity/Identity.Application/"]
COPY ["src/Identity/Identity.Domain/Identity.Domain.csproj", "src/Identity/Identity.Domain/"]
COPY ["src/Sales/Sales.Infrastructure/Sales.Infrastructure.csproj", "src/Sales/Sales.Infrastructure/"]
COPY ["src/Sales/Sales.Application/Sales.Application.csproj", "src/Sales/Sales.Application/"]
COPY ["src/Sales/Sales.Domain/Sales.Domain.csproj", "src/Sales/Sales.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Integrations.OneC/BuildingBlocks.Integrations.OneC.csproj", "src/BuildingBlocks/BuildingBlocks.Integrations.OneC/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]

RUN dotnet restore "src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj"
COPY . .
WORKDIR "/src/src/Bootstrapper/Host.Jobs/Host.Jobs"
RUN dotnet build "./Host.Jobs.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Host.Jobs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Input files of import-catalog-attributes; the job reads them by its default relative paths.
COPY --from=build ["/src/docs/catalog-attributes/dictionaries.json", "/src/docs/catalog-attributes/products_attributes.csv", "./docs/catalog-attributes/"]
ENTRYPOINT ["dotnet", "Host.Jobs.dll"]
