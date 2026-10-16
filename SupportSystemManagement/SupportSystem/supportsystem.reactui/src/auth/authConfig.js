import { PublicClientApplication } from "@azure/msal-browser";

// Microsoft Entra ID (OpenID Connect) sign-in: MSAL runs the authorization code flow with PKCE in the
// browser - there is no client secret. Entra only proves who the user is: the access token it issues for the
// API scope is exchanged once for a Support System session (see session.js), and the gateway never sees it.
// Values come from config.js at runtime (written from env vars by docker-entrypoint.d/40-app-config.sh),
// falling back to REACT_APP_* in .env.local for `npm start`.
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
    // Kept only so sign-out can end the Entra session too; cleared whenever there is no Support System session.
    cacheLocation: "sessionStorage",
  },
});

// Sign-in: OpenID Connect scopes (the ID token) plus the API scope, so the redirect also returns the access token
// to exchange. select_account lets a user whose account isn't registered try a different one.
export const loginRequest = { scopes: ["openid", "profile", "email", apiScope], prompt: "select_account" };

// Must run before rendering. Completes a sign-in redirect coming back from Entra (#code=... in the URL), if there
// is one, and returns its result (result.accessToken is for the API scope); otherwise null.
export async function initializeAuth() {
  await msalInstance.initialize();

  const result = await msalInstance.handleRedirectPromise();
  if (result?.account) {
    msalInstance.setActiveAccount(result.account);
  } else if (!msalInstance.getActiveAccount()) {
    const [account] = msalInstance.getAllAccounts();
    if (account) msalInstance.setActiveAccount(account);
  }
  return result;
}

// Drops MSAL's accounts and tokens from this tab (it doesn't sign out of Entra ID).
export async function forgetMicrosoftAccount() {
  if (!isAuthConfigured) return;
  await msalInstance.clearCache();
}
