#!/bin/sh
# Run by the nginx image's entrypoint before nginx starts. Rewrites config.js from environment
# variables so the connection string is injected at deploy time instead of baked into the build.
set -eu

# Escape backslashes and double quotes so the value is a safe JavaScript string literal.
escape() {
  printf '%s' "$1" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'
}

cat > /usr/share/nginx/html/config.js <<EOF
window.__APP_CONFIG__ = {
  appInsightsConnectionString: "$(escape "${APPLICATIONINSIGHTS_CONNECTION_STRING:-}")",
};
EOF

if [ -n "${APPLICATIONINSIGHTS_CONNECTION_STRING:-}" ]; then
  echo "40-app-config.sh: Application Insights browser telemetry enabled"
else
  echo "40-app-config.sh: APPLICATIONINSIGHTS_CONNECTION_STRING not set, browser telemetry disabled"
fi
