#!/bin/bash
set -e
echo "⏳ Starting migrations..."
# Устанавливаем утилиты
apt-get update && apt-get install -y postgresql-client

# Устанавливаем EF Tools
dotnet tool install --global dotnet-ef --version 8.*
export PATH="$PATH:/root/.dotnet/tools"

echo "📐 Running EF Core migrations..."

dotnet ef database update \
  --connection "Host=db;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
  --context CatalogDbContext \
  --project src/Infrastructure \
  --startup-project src/Presentation/Presentation

dotnet ef database update \
  --connection "Host=db;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
  --context AppIdentityDbContext \
  --project src/Infrastructure \
  --startup-project src/Presentation/Presentation

echo "✅ Migrations completed."
