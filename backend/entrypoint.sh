#!/bin/sh

echo "Waiting for DB to be ready..."

# Пример с bash и /dev/tcp (требует bash)
while ! timeout 1 bash -c "echo > /dev/tcp/$DB_HOST/$DB_PORT"; do
  echo "Waiting for DB..."
  sleep 1
done

echo "DB is ready"

dotnet ef database update --project DAL/DAL.csproj --startup-project ShiftEaseAPI/ShiftEaseAPI.csproj

exec dotnet ShiftEaseAPI.dll
