import { PublicClientApplication, EventType } from "@azure/msal-browser";

// Microsoft Entra ID (OpenID Connect) sign-in: MSAL runs the authorization code flow with PKCE in the
// browser - there is no client secret, and Entra issues the ID token (who signed in) and the access token
// the gateway validates. Values come from config.js at runtime (written from env vars by
// docker-entrypoint.d/40-app-config.sh), falling back to REACT_APP_* in .env.local for `npm start`.
const appConfig = window.__APP_CONFIG__ || {};
const tenantId = appConfig.entraTenantId || process.env.REACT_APP_ENTRA_TENANT_ID || "";
const clientId = appConfig.entraSpaClientId || process.env.REACT_APP_ENTRA_SPA_CLIENT_ID || "";
// Delegated scope exposed by the API app registration, e.g. api://<api-client-id>/access_as_user
const apiScope = appConfig.entraApiScope || process.env.REACT_APP_ENTRA_API_SCOPE || "";

export const isAuthConfigured = Boolean(tenantId && clientId && apiScope);

export const msalInstance = new PublicClientApplication({
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    // Must match a "Single-page application" redirect URI on the app registration.
    redirectUri: `${window.location.origin}/`,
    postLogoutRedirectUri: `${window.location.origin}/`,
  },
  cache: {
    // Tokens survive a page reload but not closing the tab, and aren't shared across tabs.
    cacheLocation: "sessionStorage",
  },
});

// Scopes for the access token sent to the gateway.
export const apiRequest = { scopes: [apiScope] };

// Sign-in: OpenID Connect scopes (the ID token) plus consent for the API scope in one round trip.
export const loginRequest = { scopes: ["openid", "profile", "email", apiScope] };

// Must run before rendering or acquiring tokens; keeps the signed-in account as the active one.
export async function initializeAuth() {
  await msalInstance.initialize();

  msalInstance.addEventCallback((event) => {
    if (
      (event.eventType === EventType.LOGIN_SUCCESS || event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS) &&
      event.payload?.account
    ) {
      msalInstance.setActiveAccount(event.payload.account);
    }
  });

  // Completes a sign-in redirect coming back from Entra (#code=... in the URL), if there is one.
  const result = await msalInstance.handleRedirectPromise();
  if (result?.account) {
    msalInstance.setActiveAccount(result.account);
  } else if (!msalInstance.getActiveAccount()) {
    const [account] = msalInstance.getAllAccounts();
    if (account) msalInstance.setActiveAccount(account);
  }
}
