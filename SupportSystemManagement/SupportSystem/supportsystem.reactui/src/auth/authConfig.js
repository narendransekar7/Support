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
    // Only holds the tokens of a sign-in redirect until index.js has exchanged them; cleared right after.
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

// Drops MSAL's accounts and tokens (ID, access and the unused refresh token) from this tab - called right after
// the access token has been exchanged, so no Entra token stays in sessionStorage where scripts could read it.
// It doesn't sign out of Entra ID.
export async function forgetMicrosoftAccount() {
  if (!isAuthConfigured) return;
  await msalInstance.clearCache();
}

// With MSAL's account gone, signing out of Entra ID too needs to know which account to end: only its sign-in name
// is kept, as the logout hint (it lets Entra skip its "pick an account to sign out" page). Not a credential.
const LOGOUT_HINT_KEY = "ss.microsoftLogoutHint";

export function rememberMicrosoftSignOut(account) {
  try {
    if (account?.username) sessionStorage.setItem(LOGOUT_HINT_KEY, account.username);
  } catch {
    // Storage unavailable: sign-out then just ends the Support System session.
  }
}

export function clearMicrosoftSignOut() {
  try {
    sessionStorage.removeItem(LOGOUT_HINT_KEY);
  } catch {
    // ignore
  }
}

// Ends the Entra ID session if the current Support System session came from "Sign in with Microsoft";
// Entra then redirects back to postLogoutRedirectUri. Does nothing for a password session.
export async function signOutOfMicrosoft() {
  let logoutHint = null;
  try {
    logoutHint = sessionStorage.getItem(LOGOUT_HINT_KEY);
  } catch {
    // ignore
  }
  clearMicrosoftSignOut();
  if (isAuthConfigured && logoutHint) {
    await msalInstance.logoutRedirect({ logoutHint });
  }
}
