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

# Копируем файлы проектов для восстановления зависимостей (кэширование слоев)
COPY ["KedrStore.sln", "./"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Presentation/Presentation/Presentation.csproj", "src/Presentation/Presentation/"]
COPY ["src/Presentation/Presentation.Client/Presentation.Client.csproj", "src/Presentation/Presentation.Client/"]
COPY ["src/Presentation/Presentation.Shared/Presentation.Shared.csproj", "src/Presentation/Presentation.Shared/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
# Также копируем тесты, если они нужны для сборки (в вашем исходном Dockerfile они были)
COPY ["tests/ArchitectureTests/ArchitectureTests.csproj", "tests/ArchitectureTests/"]
COPY ["tests/IntegrationTests/IntegrationTests.csproj", "tests/IntegrationTests/"]
COPY ["tests/UnitTests/UnitTests.csproj", "tests/UnitTests/"]

RUN dotnet restore "KedrStore.sln"

# Копируем все остальное
COPY . .
WORKDIR "/src/src/Presentation/Presentation"
RUN dotnet build "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# === Публикация ===
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# === Финальный образ ===
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Настройка пользователя для безопасности
RUN useradd -m kedruser && chown -R kedruser:kedruser /app
USER kedruser

ENTRYPOINT ["dotnet", "Presentation.dll"]
