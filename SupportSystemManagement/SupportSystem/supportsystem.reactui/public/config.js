// Runtime configuration, loaded by index.html before the app bundle.
// In the container this file is regenerated at startup from environment variables by
// docker-entrypoint.d/40-app-config.sh, so the same image works in every environment.
// This checked-in copy is what `npm start` serves: telemetry off, logs to the browser console only.
// Microsoft Entra ID values are left empty here - for `npm start`, put REACT_APP_ENTRA_TENANT_ID,
// REACT_APP_ENTRA_SPA_CLIENT_ID and REACT_APP_ENTRA_API_SCOPE in .env.local instead (see .env).
window.__APP_CONFIG__ = {
  appInsightsConnectionString: "",
  entraTenantId: "",
  entraSpaClientId: "",
  entraApiScope: "",
};
