import axios from "axios";

// Email/password sign-in through SS.Auth.Server.API (the alternative to Microsoft Entra ID, see authConfig.js).
// The Auth Server returns both tokens only as HttpOnly cookies - ss_access (sent with every /api call) and
// ss_refresh (sent only to /api/auth) - so this code never sees a token and an injected script can't steal one.
// Plain axios on purpose: the shared API instance (api/axios.js) refreshes on 401 and would loop back here.
const AUTH_URL = `${process.env.REACT_APP_GATEWAY_URL || ""}/api/auth`;

// CSRF guard: the APIs only accept the cookies on requests carrying this header, which a page on another
// site can't add to a request.
export const CSRF_HEADERS = { "X-Requested-With": "XMLHttpRequest" };

const post = (path, body) => axios.post(`${AUTH_URL}/${path}`, body, { headers: CSRF_HEADERS });

export async function signInWithPassword(email, password) {
  await post("login", { email, password });
}

// Concurrent 401s share one refresh: the refresh token is single-use (rotated by the server),
// so a second parallel refresh with the same cookie would be rejected as a replay.
let refreshInFlight = null;

export function refreshPasswordSession() {
  if (!refreshInFlight) {
    refreshInFlight = post("refresh-token").finally(() => {
      refreshInFlight = null;
    });
  }
  return refreshInFlight;
}

// Revokes the refresh token on the server, which also deletes both cookies.
export async function signOutPassword() {
  await post("logout");
}

// Older versions kept the tokens in localStorage - remove any leftovers.
export function clearLegacyTokens() {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("email");
}

// The session can end outside of any component (the refresh cookie expired or was revoked while an API call
// was being retried); AuthenticatedApp registers a handler that updates the Redux state.
let sessionEndedHandler = null;

export function onPasswordSessionEnded(handler) {
  sessionEndedHandler = handler;
  return () => {
    if (sessionEndedHandler === handler) sessionEndedHandler = null;
  };
}

export function notifyPasswordSessionEnded() {
  sessionEndedHandler?.();
}
