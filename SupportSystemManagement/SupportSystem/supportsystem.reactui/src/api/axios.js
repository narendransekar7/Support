// JavaScript source code
import axios from "axios";
import { CSRF_HEADERS, refreshSession, notifySessionEnded } from "../auth/session";

// Relative by default: the dev proxy (setupProxy.js), nginx or the ingress forwards /api to the gateway.
const gatewayUrl = process.env.REACT_APP_GATEWAY_URL || "";

// The session's token travels in an HttpOnly cookie the browser attaches by itself (same origin) - for both
// sign-in options; the APIs only accept that cookie together with the CSRF header.
const API = axios.create({
    baseURL: `${gatewayUrl}/api`,
    headers: CSRF_HEADERS,
});

// The access cookie expired or is missing: let the Auth Server rotate the refresh cookie into new cookies and
// retry once; if that fails the session is over.
API.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401 && !error.config._retry) {
            error.config._retry = true;
            try {
                await refreshSession();
                return API(error.config);
            } catch {
                notifySessionEnded();
            }
        }
        return Promise.reject(error);
    }
);

export default API;
