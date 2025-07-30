#!/bin/bash
set -e

echo "⏳ Waiting for database to be healthy..."
# Ждём пока Postgres станет готов
until pg_isready -h db -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  echo "Waiting for postgres..."
  sleep 2
done

echo "📦 Restoring EF Core tools..."
dotnet tool restore

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
