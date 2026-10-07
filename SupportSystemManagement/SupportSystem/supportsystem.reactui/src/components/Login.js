import React, { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useDispatch, useSelector } from "react-redux";
import { loginUser, loginWithMicrosoft } from "../features/authSlice";
import useSignedIn from "../auth/useSignedIn";
import { isAuthConfigured } from "../auth/authConfig";

// Two ways in: email/password (SS.Auth.Server.API) or Microsoft Entra ID (OpenID Connect,
// authorization code + PKCE - the button redirects to the Microsoft sign-in page and back here).
function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const dispatch = useDispatch();
  const { isLoading, error } = useSelector((state) => state.auth);
  const { signedIn, pending } = useSignedIn();

  if (signedIn) {
    return <Navigate to="/user/add" replace />;
  }

  const handleSubmit = (e) => {
    e.preventDefault();
    // On success passwordSignedIn flips and the <Navigate> above takes over.
    dispatch(loginUser({ email, password }));
  };

  const busy = isLoading || pending;

  return (
    <div style={styles.container}>
      <h2>Support System Login</h2>
      <form onSubmit={handleSubmit} style={styles.form}>
        <div style={styles.inputContainer}>
          <label htmlFor="email">Email:</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            style={styles.input}
          />
        </div>
        <div style={styles.inputContainer}>
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            style={styles.input}
          />
        </div>
        {error && <p role="alert" style={styles.error}>{typeof error === 'string' ? error : 'Login failed.'}</p>}
        <button type="submit" disabled={busy} style={styles.button}>
          {isLoading ? 'Logging in...' : 'Login'}
        </button>

        {/* Only offered when the Entra ID settings (config.js / .env.local) are present. */}
        {isAuthConfigured && (
          <>
            <div style={styles.divider}>or</div>

            <button
              type="button"
              onClick={() => dispatch(loginWithMicrosoft())}
              disabled={busy}
              style={styles.microsoftButton}
            >
              {pending ? 'Signing in...' : 'Sign in with Microsoft'}
            </button>
          </>
        )}
      </form>
    </div>
  );
}

const styles = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh',
    backgroundColor: '#f0f0f0',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    width: '300px',
    padding: '20px',
    backgroundColor: '#fff',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px',
  },
  inputContainer: {
    marginBottom: '15px',
  },
  input: {
    width: '100%',
    padding: '10px',
    borderRadius: '4px',
    border: '1px solid #ccc',
  },
  error: {
    color: '#dc2626',
    marginTop: 0,
    marginBottom: '15px',
  },
  button: {
    padding: '10px',
    backgroundColor: '#007bff',
    color: '#fff',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
  },
  divider: {
    textAlign: 'center',
    color: '#64748b',
    margin: '12px 0',
  },
  microsoftButton: {
    padding: '10px',
    backgroundColor: '#fff',
    color: '#1e293b',
    border: '1px solid #8c8c8c',
    borderRadius: '4px',
    cursor: 'pointer',
  },
};

export default Login;
