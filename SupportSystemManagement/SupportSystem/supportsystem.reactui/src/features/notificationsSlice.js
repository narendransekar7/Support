import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import API from "../api/axios";
import logger from "../telemetry/logger";

// The signed-in user's notifications from SS.Notification.API (through the gateway). The API works out the
// user from the session cookie, so these calls never send a user id.
//   GET  /notification/me?take=N    -> { unreadCount, items: [{ notificationId, type, ticketId, message, createdAt, isRead }] }
//   POST /notification/{id}/read
//   POST /notification/read-all

// take: how many to load. Omitted (the bell's poll) = the largest size asked for so far, so polling while the
// Notifications page (100) is open doesn't shrink its list back to the bell's 20.
export const fetchNotifications = createAsyncThunk("notifications/fetch", async (take, thunkAPI) => {
  const size = Math.max(take ?? 0, thunkAPI.getState().notifications.take);
  try {
    const response = await API.get("/notification/me", { params: { take: size } });
    return { ...response.data, take: size };
  } catch (error) {
    logger.warn("Loading notifications failed", { status: error.response?.status });
    return thunkAPI.rejectWithValue(error.response?.status ?? "error");
  }
});

export const markNotificationRead = createAsyncThunk("notifications/markRead", async (notificationId, thunkAPI) => {
  try {
    await API.post(`/notification/${notificationId}/read`);
    return notificationId;
  } catch (error) {
    logger.warn("Marking a notification read failed", { status: error.response?.status });
    return thunkAPI.rejectWithValue(error.response?.status ?? "error");
  }
});

export const markAllNotificationsRead = createAsyncThunk("notifications/markAllRead", async (_, thunkAPI) => {
  try {
    await API.post("/notification/read-all");
    return true;
  } catch (error) {
    logger.warn("Marking all notifications read failed", { status: error.response?.status });
    return thunkAPI.rejectWithValue(error.response?.status ?? "error");
  }
});

const notificationsSlice = createSlice({
  name: "notifications",
  initialState: {
    items: [],
    unreadCount: 0,
    take: 20,
    status: "idle", // idle | loading | loaded | failed
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchNotifications.pending, (state) => {
        if (state.status === "idle") state.status = "loading";
      })
      .addCase(fetchNotifications.fulfilled, (state, action) => {
        state.items = action.payload.items;
        state.unreadCount = action.payload.unreadCount;
        state.take = action.payload.take;
        state.status = "loaded";
      })
      .addCase(fetchNotifications.rejected, (state) => {
        state.status = "failed";
      })
      // Optimistic: update the list right away; the next poll corrects it if the call failed.
      .addCase(markNotificationRead.pending, (state, action) => {
        const item = state.items.find((n) => n.notificationId === action.meta.arg);
        if (item && !item.isRead) {
          item.isRead = true;
          state.unreadCount = Math.max(0, state.unreadCount - 1);
        }
      })
      .addCase(markAllNotificationsRead.pending, (state) => {
        state.items.forEach((n) => { n.isRead = true; });
        state.unreadCount = 0;
      })
      // Forget the previous user's notifications when the session ends (sign-out or expired session).
      .addMatcher(
        (action) => action.type === "auth/sessionEnded" || action.type === "auth/logout/fulfilled",
        (state) => {
          state.items = [];
          state.unreadCount = 0;
          state.take = 20;
          state.status = "idle";
        }
      );
  },
});

export default notificationsSlice.reducer;
