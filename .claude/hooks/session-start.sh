#!/bin/bash
# Prepares a Claude Code cloud session: installs the .NET 10 SDK and restores NuGet packages.
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

cd "$CLAUDE_PROJECT_DIR"

# Ubuntu's package matches global.json (10.0.1xx with rollForward latestFeature); builds.dotnet.microsoft.com is not reachable here.
if ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
  apt-get update -qq
  DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0
fi

echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> "$CLAUDE_ENV_FILE"
echo 'export DOTNET_NOLOGO=1' >> "$CLAUDE_ENV_FILE"

dotnet restore KedrStore.sln
