import React from 'react';
import ReactDOM from 'react-dom/client';
// Imported before App so Application Insights (if configured) starts before any app code runs.
import logger from './telemetry/logger';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import ErrorBoundary from './components/ErrorBoundary';
import { initializeAuth, isAuthConfigured, forgetMicrosoftAccount } from './auth/authConfig';
import store from './app/store';
import { completeMicrosoftSignIn, restoreSession } from './features/authSlice';


const root = ReactDOM.createRoot(document.getElementById('root'));

const render = (content) =>
  root.render(
    <React.StrictMode>
      <ErrorBoundary>{content}</ErrorBoundary>
    </React.StrictMode>
  );

// Before the first render: MSAL must be initialized and, when coming back from the Microsoft sign-in page, its
// access token exchanged for a Support System session; then the session (HttpOnly cookies, either sign-in option)
// is restored, so protected pages don't bounce to the login page.
// Without Entra ID settings only the email/password login is offered.
if (!isAuthConfigured) {
  logger.warn("Microsoft Entra ID sign-in is not configured (entraTenantId / entraSpaClientId / entraApiScope); only password login is available");
}

async function startSession() {
  try {
    const microsoftSignIn = await initializeAuth();
    if (microsoftSignIn) {
      await store.dispatch(completeMicrosoftSignIn(microsoftSignIn.accessToken));
    }
  } catch (error) {
    logger.error("Microsoft Entra ID sign-in failed", error);
  }

  const restored = await store.dispatch(restoreSession());
  if (restoreSession.rejected.match(restored)) {
    // No session (e.g. the Microsoft account isn't registered): MSAL's account is of no use, and keeping it
    // would make a later sign-out also sign out of Microsoft.
    await forgetMicrosoftAccount().catch((error) => logger.warn("Clearing the Microsoft account failed", error));
  }
}

startSession().finally(() => render(<App />));

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals((metric) => logger.debug(`Web vital ${metric.name}`, { value: metric.value }));
