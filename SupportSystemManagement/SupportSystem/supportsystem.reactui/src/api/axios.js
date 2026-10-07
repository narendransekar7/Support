// JavaScript source code
import axios from "axios";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { msalInstance, apiRequest } from "../auth/authConfig";
import { getPasswordToken, refreshPasswordToken, clearPasswordSession } from "../auth/passwordAuth";

// Relative by default: the dev proxy (setupProxy.js), nginx or the ingress forwards /api to the gateway.
const gatewayUrl = process.env.REACT_APP_GATEWAY_URL || "";

const API = axios.create({
    baseURL: `${gatewayUrl}/api`,
});

// Gets a Microsoft Entra ID access token for the API. MSAL serves it from its cache and renews it
// silently (refresh token) when it is close to expiry; only when Entra needs the user again
// (session ended, consent, MFA) does it fall back to a full-page redirect to sign in.
async function getEntraAccessToken(forceRefresh = false) {
    const account = msalInstance.getActiveAccount();
    if (!account) return null;

    try {
        const result = await msalInstance.acquireTokenSilent({ ...apiRequest, account, forceRefresh });
        return result.accessToken;
    } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
            await msalInstance.acquireTokenRedirect({ ...apiRequest, account });
        }
        throw error;
    }
}

// Two sign-in options: a password-login session (SS.Auth.Server.API token in localStorage) or Microsoft Entra ID.
API.interceptors.request.use(
    async (config) => {
        const token = getPasswordToken() || (await getEntraAccessToken());

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

API.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401 && !error.config._retry) {
            error.config._retry = true;

            if (getPasswordToken()) {
                // Password login: rotate the refresh token for a new access token, or end the session.
                try {
                    const token = await refreshPasswordToken();
                    error.config.headers.Authorization = `Bearer ${token}`;
                    return API(error.config);
                } catch {
                    clearPasswordSession();
                    window.location.assign("/");
                }
            } else {
                // Entra ID: a token MSAL still considered valid was rejected (e.g. revoked) - retry once with a fresh one.
                const token = await getEntraAccessToken(true);
                if (token) {
                    error.config.headers.Authorization = `Bearer ${token}`;
                    return API(error.config);
                }
            }
        }
        return Promise.reject(error);
    }
);

export default API;
