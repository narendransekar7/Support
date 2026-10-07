import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import API from "../api/axios";
import { msalInstance, loginRequest } from "../auth/authConfig";
import { isPasswordSignedIn, signInWithPassword, signOutPassword } from "../auth/passwordAuth";
import logger from "../telemetry/logger";

// Two sign-in options:
//   - "password": email/password through SS.Auth.Server.API (tokens handled in auth/passwordAuth.js)
//   - "entra":    Microsoft Entra ID via MSAL (tokens handled by MSAL, see auth/authConfig.js)
// An Entra session is reported by MSAL (useIsAuthenticated); this slice tracks a password session itself,
// plus the Support System profile (id, role, name) of whoever is signed in.

// Email/password sign-in.
export const loginUser = createAsyncThunk("auth/login", async (credentials, thunkAPI) => {
  try {
    await signInWithPassword(credentials.email, credentials.password);
    logger.info("Login succeeded", { method: "password" });
    return true;
  } catch (error) {
    logger.warn("Login failed", { method: "password", status: error.response?.status });
    return thunkAPI.rejectWithValue(error.response?.data || "Login failed.");
  }
});

// Redirects to the Microsoft sign-in page; the app reloads on return and MSAL completes the login.
export const loginWithMicrosoft = createAsyncThunk("auth/loginWithMicrosoft", async () => {
  await msalInstance.loginRedirect(loginRequest);
});

// The user's Support System account, looked up by the API from the email claim in the access token.
export const fetchUser = createAsyncThunk("auth/fetchUser", async (_, thunkAPI) => {
  try {
    const response = await API.get("/user/me");
    logger.setUser(response.data.userId);
    return response.data; // { userId, name, role }
  } catch (error) {
    const status = error.response?.status;
    if (status === 403) {
      logger.warn("Signed-in account has no Support System account");
    } else {
      logger.error("Fetching the signed-in user failed", error);
    }
    return thunkAPI.rejectWithValue({ status, message: error.response?.data || error.message });
  }
});

// Password session: revoke the refresh token and return to the login page.
// Entra session: end the MSAL and Entra ID sessions; Entra redirects back to the login page.
export const logoutUser = createAsyncThunk("auth/logout", async () => {
  logger.clearUser();
  if (isPasswordSignedIn()) {
    try {
      await signOutPassword();
    } catch (error) {
      // The local session is cleared regardless; the refresh token just stays valid until it expires.
      logger.warn("Logout: revoking the refresh token failed", { status: error.response?.status });
    }
    return "password";
  }
  await msalInstance.logoutRedirect({ account: msalInstance.getActiveAccount() });
  return "entra";
});

const authSlice = createSlice({
  name: "auth",
  initialState: {
    passwordSignedIn: isPasswordSignedIn(),
    userid: null,
    userrole: null,
    username: null,
    // idle | loading | loaded | notRegistered | failed
    profileStatus: "idle",
    isLoading: false,
    error: null,
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state) => {
        state.isLoading = false;
        state.passwordSignedIn = true;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload;
      })
      .addCase(fetchUser.pending, (state) => {
        state.profileStatus = "loading";
      })
      .addCase(fetchUser.fulfilled, (state, action) => {
        state.userid = action.payload.userId;
        state.userrole = action.payload.role;
        state.username = action.payload.name;
        state.profileStatus = "loaded";
      })
      .addCase(fetchUser.rejected, (state, action) => {
        state.profileStatus = action.payload?.status === 403 ? "notRegistered" : "failed";
      })
      .addCase(logoutUser.pending, (state) => {
        state.userid = null;
        state.userrole = null;
        state.username = null;
        state.profileStatus = "idle";
      })
      .addCase(logoutUser.fulfilled, (state) => {
        state.passwordSignedIn = false;
      });
  },
});

export default authSlice.reducer;
