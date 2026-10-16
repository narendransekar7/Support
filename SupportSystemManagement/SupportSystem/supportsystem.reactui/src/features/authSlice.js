import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import API from "../api/axios";
import { msalInstance, loginRequest } from "../auth/authConfig";
import { clearLegacyTokens, exchangeMicrosoftToken, signInWithPassword, signOut } from "../auth/session";
import logger from "../telemetry/logger";

// Two sign-in options, one kind of session:
//   - email/password through SS.Auth.Server.API
//   - Microsoft Entra ID via MSAL (auth/authConfig.js), whose access token is exchanged at SS.Auth.Server.API
// Both end in the same Support System session (HttpOnly cookies, see auth/session.js). This slice tracks it,
// plus the Support System profile (id, role, name) of whoever is signed in.

// At start-up: the session cookies can't be read by JavaScript, so ask the API whether they still make a
// session. /user/me succeeds with a valid access cookie, or after api/axios.js has refreshed it.
export const restoreSession = createAsyncThunk("auth/restoreSession", async (_, thunkAPI) => {
  clearLegacyTokens();
  try {
    const response = await API.get("/user/me");
    logger.setUser(response.data.userId);
    return response.data; // { userId, name, role }
  } catch (error) {
    return thunkAPI.rejectWithValue(error.response?.status ?? "error"); // 401: not signed in
  }
});

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

// Redirects to the Microsoft sign-in page; the app reloads on return and index.js completes the login.
export const loginWithMicrosoft = createAsyncThunk("auth/loginWithMicrosoft", async () => {
  await msalInstance.loginRedirect(loginRequest);
});

// Back from Microsoft: swap the Entra access token for a Support System session (restoreSession then loads it).
export const completeMicrosoftSignIn = createAsyncThunk("auth/completeMicrosoftSignIn", async (accessToken, thunkAPI) => {
  try {
    await exchangeMicrosoftToken(accessToken);
    logger.info("Login succeeded", { method: "microsoft" });
    return true;
  } catch (error) {
    logger.warn("Login failed", { method: "microsoft", status: error.response?.status });
    return thunkAPI.rejectWithValue(error.response?.data || "Microsoft sign-in failed.");
  }
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

// Revokes the session's refresh token and returns to the login page. Signed in with Microsoft: also ends the
// Entra ID session, and Entra redirects back to the login page.
export const logoutUser = createAsyncThunk("auth/logout", async () => {
  logger.clearUser();
  try {
    await signOut();
  } catch (error) {
    // The app signs out regardless; the cookies stay until they expire and the refresh token stays valid.
    logger.warn("Logout: revoking the refresh token failed", { status: error.response?.status });
  }
  const account = msalInstance.getActiveAccount();
  if (account) {
    try {
      await msalInstance.logoutRedirect({ account });
    } catch (error) {
      logger.warn("Logout: Microsoft sign-out failed", error);
    }
  }
});

const authSlice = createSlice({
  name: "auth",
  initialState: {
    signedIn: false, // set by restoreSession / loginUser
    userid: null,
    userrole: null,
    username: null,
    // idle | loading | loaded | notRegistered | failed | signingOut
    profileStatus: "idle",
    isLoading: false,
    redirectingToMicrosoft: false,
    error: null,
  },
  reducers: {
    // The session couldn't be refreshed any more (see notifySessionEnded).
    sessionEnded(state) {
      state.signedIn = false;
      state.userid = null;
      state.userrole = null;
      state.username = null;
      state.profileStatus = "idle";
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(restoreSession.fulfilled, (state, action) => {
        state.signedIn = true;
        state.userid = action.payload.userId;
        state.userrole = action.payload.role;
        state.username = action.payload.name;
        state.profileStatus = "loaded";
      })
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state) => {
        state.isLoading = false;
        state.signedIn = true;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload;
      })
      .addCase(loginWithMicrosoft.pending, (state) => {
        state.redirectingToMicrosoft = true;
        state.error = null;
      })
      .addCase(loginWithMicrosoft.rejected, (state) => {
        state.redirectingToMicrosoft = false;
        state.error = "Couldn't open the Microsoft sign-in page.";
      })
      .addCase(completeMicrosoftSignIn.rejected, (state, action) => {
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
      // "signingOut", not "idle": the user still counts as signed in until logout finishes, and an idle
      // profile would make AuthenticatedApp fetch it again mid-logout.
      .addCase(logoutUser.pending, (state) => {
        state.userid = null;
        state.userrole = null;
        state.username = null;
        state.profileStatus = "signingOut";
      })
      .addCase(logoutUser.fulfilled, (state) => {
        state.signedIn = false;
        state.profileStatus = "idle";
      });
  },
});

export const { sessionEnded } = authSlice.actions;

export default authSlice.reducer;
