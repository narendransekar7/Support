import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";
import { jwtDecode } from "jwt-decode";

// API Base URL
// Relative by default: the dev proxy (setupProxy.js), nginx or the ingress forwards /api to the gateway.
const API_URL = `${process.env.REACT_APP_GATEWAY_URL || ""}/api`;

// Async thunk for login
export const loginUser = createAsyncThunk("auth/login", async (credentials, thunkAPI) => {
  try {
    const response = await axios.post(`${API_URL}/auth/login`, credentials);
	const token = response.data.token;
	if (token) {
		const decodedToken = jwtDecode(token);
		//console.log(user); // { sub: "12345", email: "testuser@example.com", role: "Admin", ... }
		localStorage.setItem("email", decodedToken.email);
		localStorage.setItem("token", token); // Store token
		localStorage.setItem("refreshToken", response.data.refreshToken);
	}
	
    return response.data; // { token, user }
  } catch (error) {
    return thunkAPI.rejectWithValue(error.response.data);
  }
});

// Async thunk for fetching user details
export const fetchUser = createAsyncThunk("auth/fetchUser", async (_, thunkAPI) => {
  try {
	  debugger;
    const token = localStorage.getItem("token");
	const email = localStorage.getItem("email");
    const response = await axios.get(`${API_URL}/user/fetchuser/${email}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.data; // { id, name, role }
  } catch (error) {
    return thunkAPI.rejectWithValue(error.response.data);
  }
});

export const refreshAccessToken = createAsyncThunk("auth/refreshToken", async (_, thunkAPI) => {
    try {
        const refreshToken = localStorage.getItem("refreshToken");
        const userId = thunkAPI.getState().auth.userid;
        if (!refreshToken) throw new Error("No refresh token found");

        const response = await axios.post(`${API_URL}/auth/refresh-token`, { userId, refreshToken });

        localStorage.setItem("token", response.data.token);
        localStorage.setItem("refreshToken", response.data.refreshToken);

        return response.data;
    } catch (error) {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        return thunkAPI.rejectWithValue("Session expired, please log in again.");
    }
});

// ✅ Logout thunk that calls backend API
export const logoutUser = createAsyncThunk("auth/logout", async (_, thunkAPI) => {
    try {
        const token = localStorage.getItem("token");
        const refreshToken = localStorage.getItem("refreshToken");
        const email = localStorage.getItem("email");

        if (!token || !refreshToken) {
            // No valid session
            localStorage.clear();
            return true;
        }

        const logoutPayload = {
            email: email,
            refreshToken: refreshToken,
        };
        
       
        const userId = thunkAPI.getState().auth.userid;
        const response = await axios.post(`${API_URL}/auth/logout`, { userId, refreshToken });

        //const response = await fetch(`${API_URL}/auth/logout`, {
        //    method: "POST",
        //    headers: {
        //        "Content-Type": "application/json",
        //        Authorization: `Bearer ${token}`, // Optional if your logout requires auth
        //    },
        //    body: logoutPayload,
        //});

        if (!response.ok) {
            const errorText = await response.text();
            console.error("Logout failed:", errorText);
            return thunkAPI.rejectWithValue(errorText || "Logout failed");
        }

        // ✅ Clear local storage on success
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("email");

        return true;
    } catch (error) {
        console.error("Logout error:", error);
        return thunkAPI.rejectWithValue(error.message);
    }
});



const authSlice = createSlice({
  name: "auth",
  initialState: {
    userid: null,
	userrole:null,
	username:null,
    token: localStorage.getItem("token") || null,
	refreshToken: localStorage.getItem("refreshToken") || null,
    isLoading: false,
    error: null,
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.token = action.payload.token;
		state.refreshToken = action.payload.refreshToken;
       
        state.error = null;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload;
      })
      .addCase(refreshAccessToken.fulfilled, (state, action) => {
        state.token = action.payload.token;
        state.refreshToken = action.payload.refreshToken;
      })
      .addCase(fetchUser.fulfilled, (state, action) => {
		  debugger;
         state.userid = action.payload.userId;
		 state.userrole = action.payload.role;
		 state.username = action.payload.name;
      })
      .addCase(logoutUser.fulfilled, (state) => {
		state.token = null;
		state.refreshToken = null;
        state.userid = null;
		state.userrole = null;
		state.username = null;
      });
  },
});

export default authSlice.reducer;
