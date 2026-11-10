#!/usr/bin/env bash
# Provisions the Azure Function App for SS.Functions (infra/function-app/main.bicep) and deploys the code to it.
# Used from a developer machine and by the Jenkins deploy. Safe to re-run: Bicep updates in place, and each
# run uploads a new zip package.
#
#   scripts/deploy-function-app.sh <resource-group> [location]
#
# Environment:
#   DB_CONNECTION_STRING                 required - the SupportSystem Azure SQL database
#   AZURE_SERVICE_BUS_CONNECTION_STRING  required - Service Bus namespace, Manage rights
#   FUNCTION_APP_NAME                    optional - default: ss-functions-<unique suffix of the resource group>
#   SLA_BREACH_CHECKER_SCHEDULE          optional - NCRONTAB, default "0 */5 * * * *"
#   SKIP_INFRA=true                      optional - only redeploy the code (the app must already exist)
#
# Run from SupportSystemManagement/SupportSystem, signed in with `az login` (Contributor on the resource group).
# Builds with the local .NET 8 SDK, or in the dotnet/sdk container when there's no `dotnet` (e.g. a Jenkins agent).
set -euo pipefail

RESOURCE_GROUP="${1:?usage: scripts/deploy-function-app.sh <resource-group> [location]}"
LOCATION="${2:-}"
: "${DB_CONNECTION_STRING:?set DB_CONNECTION_STRING}"
: "${AZURE_SERVICE_BUS_CONNECTION_STRING:?set AZURE_SERVICE_BUS_CONNECTION_STRING}"
SCHEDULE="${SLA_BREACH_CHECKER_SCHEDULE:-0 */5 * * * *}"

PROJECT="SS.Functions/SS.Functions.csproj"
OUT_DIR=".publish/ss-functions"
ZIP_PATH=".publish/ss-functions.zip"

if [[ -n "$LOCATION" ]] && ! az group show -n "$RESOURCE_GROUP" >/dev/null 2>&1; then
  echo "== Creating resource group $RESOURCE_GROUP in $LOCATION"
  az group create -n "$RESOURCE_GROUP" -l "$LOCATION" -o none
fi

if [[ "${SKIP_INFRA:-false}" != "true" ]]; then
  echo "== Provisioning the Function App (infra/function-app/main.bicep)"
  params=(dbConnectionString="$DB_CONNECTION_STRING"
          serviceBusConnectionString="$AZURE_SERVICE_BUS_CONNECTION_STRING"
          slaBreachCheckerSchedule="$SCHEDULE")
  [[ -n "$LOCATION" ]] && params+=(location="$LOCATION")
  [[ -n "${FUNCTION_APP_NAME:-}" ]] && params+=(functionAppName="$FUNCTION_APP_NAME")
  # The deployment name stays the same so the outputs are easy to read back; secure params aren't stored in its history.
  FUNCTION_APP_NAME=$(az deployment group create -g "$RESOURCE_GROUP" -n ss-functions \
    -f infra/function-app/main.bicep -p "${params[@]}" \
    --query properties.outputs.functionAppName.value -o tsv)
fi
: "${FUNCTION_APP_NAME:?set FUNCTION_APP_NAME when SKIP_INFRA=true}"
echo "Function App: $FUNCTION_APP_NAME"

echo "== Publishing $PROJECT (Release)"
rm -rf "$OUT_DIR" "$ZIP_PATH"
mkdir -p .publish
if command -v dotnet >/dev/null 2>&1; then
  dotnet publish "$PROJECT" -c Release -o "$OUT_DIR"
else
  docker run --rm -u "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp \
    -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
    dotnet publish "$PROJECT" -c Release -o "$OUT_DIR"
fi

# The zip must contain the publish folder's *contents* at its root, including the hidden .azurefunctions folder.
echo "== Packaging $ZIP_PATH"
if command -v zip >/dev/null 2>&1; then
  (cd "$OUT_DIR" && zip -qr "$OLDPWD/$ZIP_PATH" .)
else
  # First interpreter that really runs (on Windows "python3" can be the Microsoft Store stub).
  PY=""
  for candidate in python3 python; do
    if "$candidate" -c "" >/dev/null 2>&1; then PY="$candidate"; break; fi
  done
  [[ -n "$PY" ]] || { echo "Need zip or Python to build the package" >&2; exit 1; }
  "$PY" -c 'import shutil,sys; shutil.make_archive(sys.argv[1], "zip", sys.argv[2])' "${ZIP_PATH%.zip}" "$OUT_DIR"
fi

echo "== Deploying to $FUNCTION_APP_NAME"
az functionapp deployment source config-zip -g "$RESOURCE_GROUP" -n "$FUNCTION_APP_NAME" --src "$ZIP_PATH" -o none

echo "== Functions now in $FUNCTION_APP_NAME"
az functionapp function list -g "$RESOURCE_GROUP" -n "$FUNCTION_APP_NAME" --query "[].name" -o tsv
