// Browser logging + telemetry.
//
// Every call writes to the browser console (level-filtered). When an Application Insights connection
// string is configured, the same entries are also sent to Application Insights, together with page
// views, unhandled exceptions and /api calls. The SDK adds W3C `traceparent` headers to /api requests,
// so a click in the UI and the backend spans/logs it caused (Gateway -> Ticket API -> RabbitMQ saga ->
// Email) show up as one end-to-end transaction.
//
// Connection string source (first one set wins):
//   1. window.__APP_CONFIG__.appInsightsConnectionString - public/config.js, rewritten at container
//      start from the APPLICATIONINSIGHTS_CONNECTION_STRING env var (docker-entrypoint.d/40-app-config.sh),
//      so one image works in every environment.
//   2. REACT_APP_APPINSIGHTS_CONNECTION_STRING - build-time, handy for `npm start` via .env.local.
import { ApplicationInsights, SeverityLevel } from "@microsoft/applicationinsights-web";

const runtimeConfig = window.__APP_CONFIG__ || {};
const connectionString =
    runtimeConfig.appInsightsConnectionString || process.env.REACT_APP_APPINSIGHTS_CONNECTION_STRING || "";

const LEVELS = { debug: 0, info: 1, warn: 2, error: 3 };
const minLevel =
    LEVELS[runtimeConfig.logLevel] ?? (process.env.NODE_ENV === "production" ? LEVELS.info : LEVELS.debug);

let appInsights = null;
if (connectionString) {
    try {
        appInsights = new ApplicationInsights({
            config: {
                connectionString,
                enableAutoRouteTracking: true, // page view per react-router navigation
                disableFetchTracking: false,
                enableRequestHeaderTracking: false, // never capture Authorization headers
                enableResponseHeaderTracking: false,
            },
        });
        appInsights.loadAppInsights();
        appInsights.addTelemetryInitializer((item) => {
            item.tags = item.tags || {};
            item.tags["ai.cloud.role"] = "ss-react-ui"; // node name in the Application Map
        });
        appInsights.trackPageView();
    } catch (e) {
        // Telemetry must never break the app.
        appInsights = null;
        console.warn("Application Insights failed to initialise", e);
    }
}

const SEVERITY = {
    debug: SeverityLevel.Verbose,
    info: SeverityLevel.Information,
    warn: SeverityLevel.Warning,
    error: SeverityLevel.Error,
};

// Keys whose values must never leave the browser in a log entry.
const SENSITIVE = /pass(word)?|token|authorization|secret/i;

function sanitize(properties) {
    if (!properties) return undefined;
    const clean = {};
    for (const [key, value] of Object.entries(properties)) {
        if (SENSITIVE.test(key)) continue;
        clean[key] = typeof value === "object" && value !== null ? JSON.stringify(value) : String(value);
    }
    return clean;
}

function write(level, message, properties) {
    if (LEVELS[level] < minLevel) return;
    const props = sanitize(properties);

    const consoleFn = level === "debug" ? console.debug : console[level];
    props ? consoleFn(`[${level}] ${message}`, props) : consoleFn(`[${level}] ${message}`);

    appInsights?.trackTrace({ message, severityLevel: SEVERITY[level] }, props);
}

const logger = {
    debug: (message, properties) => write("debug", message, properties),
    info: (message, properties) => write("info", message, properties),
    warn: (message, properties) => write("warn", message, properties),

    /** Logs an error; pass the caught Error as `error` so its stack is kept in Application Insights. */
    error: (message, error, properties) => {
        const details = { ...properties };
        if (error?.message) details.error = error.message;
        if (error?.response?.status) details.status = error.response.status;
        const traceId = error?.response?.headers?.["x-trace-id"];
        if (traceId) details.traceId = traceId; // backend trace for this failed call
        write("error", message, details);

        if (appInsights && error instanceof Error) {
            appInsights.trackException(
                { exception: error, severityLevel: SeverityLevel.Error },
                sanitize({ message, ...details })
            );
        }
    },

    /** Tags all subsequent telemetry with the signed-in user (id only - no email). */
    setUser: (userId) => {
        if (userId) appInsights?.setAuthenticatedUserContext(String(userId), undefined, true);
    },
    clearUser: () => appInsights?.clearAuthenticatedUserContext(),
};

export default logger;
