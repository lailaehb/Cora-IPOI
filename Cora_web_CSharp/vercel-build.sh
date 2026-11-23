#!/usr/bin/env bash
# Vercel no trae .NET instalado: este script lo descarga, compila la web y deja el resultado en salida/wwwroot.
set -euo pipefail
echo "→ Descargando .NET 10…"
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --channel 10.0 --install-dir "$PWD/.dotnet"
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
echo "→ Compilando Cora…"
dotnet publish Cora.Web.csproj -c Release -o salida
# Número de versión (el commit de GitHub): la app lo consulta para saber si hay una versión nueva y recargarse sola
echo "${VERCEL_GIT_COMMIT_SHA:-$(date +%s)}" > salida/wwwroot/version.txt
echo "→ Listo: salida/wwwroot"
