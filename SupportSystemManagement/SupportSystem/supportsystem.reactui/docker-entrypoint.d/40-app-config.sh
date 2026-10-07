#!/bin/sh
# Run by the nginx image's entrypoint before nginx starts. Rewrites config.js from environment
# variables so the connection string and Microsoft Entra ID sign-in settings (tenant id, SPA client id,
# API scope - none of them secrets) are injected at deploy time instead of baked into the build.
set -eu

# Escape backslashes and double quotes so the value is a safe JavaScript string literal.
escape() {
  printf '%s' "$1" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'
}

cat > /usr/share/nginx/html/config.js <<EOF
window.__APP_CONFIG__ = {
  appInsightsConnectionString: "$(escape "${APPLICATIONINSIGHTS_CONNECTION_STRING:-}")",
  entraTenantId: "$(escape "${ENTRA_TENANT_ID:-}")",
  entraSpaClientId: "$(escape "${ENTRA_SPA_CLIENT_ID:-}")",
  entraApiScope: "$(escape "${ENTRA_API_SCOPE:-}")",
};
EOF

if [ -n "${APPLICATIONINSIGHTS_CONNECTION_STRING:-}" ]; then
  echo "40-app-config.sh: Application Insights browser telemetry enabled"
else
  echo "40-app-config.sh: APPLICATIONINSIGHTS_CONNECTION_STRING not set, browser telemetry disabled"
fi

if [ -z "${ENTRA_TENANT_ID:-}" ] || [ -z "${ENTRA_SPA_CLIENT_ID:-}" ] || [ -z "${ENTRA_API_SCOPE:-}" ]; then
  echo "40-app-config.sh: ENTRA_TENANT_ID / ENTRA_SPA_CLIENT_ID / ENTRA_API_SCOPE not all set, only the email/password login is offered"
fi
