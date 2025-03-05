import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";
import { jwtDecode } from "jwt-decode";

// API Base URL
const API_URL = "http://localhost:5145/api";

// Async thunk for login
export const loginUser = createAsyncThunk("auth/login", async (credentials, thunkAPI) => {
  try {
	  debugger;
	
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

// Logout action
export const logoutUser = createAsyncThunk("auth/logout", async () => {
  localStorage.removeItem("token"); // Remove token
  localStorage.removeItem("refreshToken");
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
