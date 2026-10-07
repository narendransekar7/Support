import axios from "axios";
import { jwtDecode } from "jwt-decode";

// Email/password sign-in through SS.Auth.Server.API (the alternative to Microsoft Entra ID, see authConfig.js).
// It returns an HS256 access token + a rotating refresh token, kept in localStorage as before.
// Plain axios on purpose: the shared API instance (api/axios.js) attaches tokens and would loop back here.
const AUTH_URL = `${process.env.REACT_APP_GATEWAY_URL || ""}/api/auth`;
const TOKEN_KEY = "token";
const REFRESH_TOKEN_KEY = "refreshToken";

export const getPasswordToken = () => localStorage.getItem(TOKEN_KEY);
export const isPasswordSignedIn = () => Boolean(getPasswordToken());

// The Auth Server's tokens carry the user id as a "UserId" claim; decoding works even once the token has expired.
function userIdFromToken(token) {
  try {
    return jwtDecode(token).UserId ?? null;
  } catch {
    return null;
  }
}

export function clearPasswordSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem("email"); // written by older versions of the app
}

export async function signInWithPassword(email, password) {
  const { data } = await axios.post(`${AUTH_URL}/login`, { email, password });
  localStorage.setItem(TOKEN_KEY, data.token);
  localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
}

// Concurrent 401s share one refresh: the refresh token is single-use (rotated by the server),
// so a second parallel refresh with the same token would be rejected as a replay.
let refreshInFlight = null;

export function refreshPasswordToken() {
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      const token = getPasswordToken();
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      if (!token || !refreshToken) throw new Error("No refresh token found");

      const { data } = await axios.post(`${AUTH_URL}/refresh-token`, { userId: userIdFromToken(token), refreshToken });
      localStorage.setItem(TOKEN_KEY, data.token);
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
      return data.token;
    })().finally(() => {
      refreshInFlight = null;
    });
  }
  return refreshInFlight;
}

// Revokes the refresh token on the server; the local session is cleared even if that call fails.
export async function signOutPassword() {
  const token = getPasswordToken();
  const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
  try {
    if (token && refreshToken) {
      await axios.post(`${AUTH_URL}/logout`, { userId: userIdFromToken(token), refreshToken });
    }
  } finally {
    clearPasswordSession();
  }
}
