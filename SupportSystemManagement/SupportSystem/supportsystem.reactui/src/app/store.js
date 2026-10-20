import { configureStore } from "@reduxjs/toolkit";
import authReducer from "../features/authSlice";
import notificationsReducer from "../features/notificationsSlice";

const store = configureStore({
  reducer: {
    auth: authReducer,
    notifications: notificationsReducer,
  },
});

export default store;
