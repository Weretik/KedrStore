# === Runtime образ (production) ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
USER root
RUN useradd -m kedruser
USER kedruser

# === Build  ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# === restore .NET ===
COPY KedrStore.sln ./
COPY src/Domain/*.csproj src/Domain/
COPY src/Application/*.csproj src/Application/
COPY src/Infrastructure/*.csproj src/Infrastructure/
COPY src/Presentation/Presentation/*.csproj src/Presentation/Presentation/
COPY src/Presentation/Presentation.Client/*.csproj src/Presentation/Presentation.Client/
COPY src/Presentation/Presentation.Shared/*.csproj src/Presentation/Presentation.Shared/
COPY tests/UnitTests/*.csproj tests/UnitTests/
COPY tests/IntegrationTests/*.csproj tests/IntegrationTests/
COPY tests/ArchitectureTests/*.csproj tests/ArchitectureTests/

RUN dotnet restore "KedrStore.sln"

# === Копируем весь код ===
COPY . .

# === Публикация .NET ===
RUN dotnet publish src/Presentation/Presentation/Presentation.csproj -c Release -o /app --no-restore

# === Финальный образ ===
FROM base AS final
WORKDIR /app
COPY --from=build /app ./
USER root
RUN chown -R kedruser:kedruser /app
USER kedruser

ENTRYPOINT ["dotnet","Presentation.dll"]
