import React from 'react';
import ReactDOM from 'react-dom/client';
// Imported before App so Application Insights (if configured) starts before any app code runs.
import logger from './telemetry/logger';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import ErrorBoundary from './components/ErrorBoundary';
import { initializeAuth, isAuthConfigured, forgetMicrosoftAccount, rememberMicrosoftSignOut, clearMicrosoftSignOut } from './auth/authConfig';
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
      const exchanged = await store.dispatch(completeMicrosoftSignIn(microsoftSignIn.accessToken));
      if (completeMicrosoftSignIn.fulfilled.match(exchanged)) {
        rememberMicrosoftSignOut(microsoftSignIn.account);
      }
    }
  } catch (error) {
    logger.error("Microsoft Entra ID sign-in failed", error);
  }
  // The Entra tokens have done their job (or the exchange failed): don't leave them in sessionStorage. This also
  // drops MSAL accounts left over from older versions of the app.
  await forgetMicrosoftAccount().catch((error) => logger.warn("Clearing the Microsoft account failed", error));

  const restored = await store.dispatch(restoreSession());
  if (restoreSession.rejected.match(restored)) {
    // No session (e.g. the Microsoft account isn't registered): nothing to sign out of Microsoft for later.
    clearMicrosoftSignOut();
  }
}

startSession().finally(() => render(<App />));

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals((metric) => logger.debug(`Web vital ${metric.name}`, { value: metric.value }));
