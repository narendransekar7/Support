// Runtime configuration, loaded by index.html before the app bundle.
// In the container this file is regenerated at startup from environment variables by
// docker-entrypoint.d/40-app-config.sh, so the same image works in every environment.
// This checked-in copy is what `npm start` serves: telemetry off, logs to the browser console only.
window.__APP_CONFIG__ = {
  appInsightsConnectionString: "",
};
