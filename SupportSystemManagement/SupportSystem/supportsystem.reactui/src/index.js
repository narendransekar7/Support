import React from 'react';
import ReactDOM from 'react-dom/client';
// Imported before App so Application Insights (if configured) starts before any app code runs.
import logger from './telemetry/logger';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import ErrorBoundary from './components/ErrorBoundary';
import { initializeAuth, isAuthConfigured } from './auth/authConfig';
import store from './app/store';
import { restorePasswordSession } from './features/authSlice';


const root = ReactDOM.createRoot(document.getElementById('root'));

const render = (content) =>
  root.render(
    <React.StrictMode>
      <ErrorBoundary>{content}</ErrorBoundary>
    </React.StrictMode>
  );

// Before the first render: MSAL must be initialized (and any sign-in redirect from Entra ID processed), and an
// existing password-login session (HttpOnly cookies) restored, so protected pages don't bounce to the login page.
// Without Entra ID settings only the email/password login is offered.
if (!isAuthConfigured) {
  logger.warn("Microsoft Entra ID sign-in is not configured (entraTenantId / entraSpaClientId / entraApiScope); only password login is available");
}
initializeAuth()
  .catch((error) => logger.error("Microsoft Entra ID sign-in initialization failed", error))
  .then(() => store.dispatch(restorePasswordSession()))
  .finally(() => render(<App />));

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals((metric) => logger.debug(`Web vital ${metric.name}`, { value: metric.value }));
