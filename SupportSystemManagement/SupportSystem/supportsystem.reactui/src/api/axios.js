// JavaScript source code
import axios from "axios";
import store from "../app/store";
import { refreshAccessToken } from "../features/authSlice";

const API = axios.create({
    baseURL: "https://localhost:44345/api",
});

API.interceptors.request.use(
    async (config) => {
        const token = localStorage.getItem("token");

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

API.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401 && !error.config._retry) {
            error.config._retry = true;
            try {
                await store.dispatch(refreshAccessToken()).unwrap();
                const newToken = localStorage.getItem("token");
                error.config.headers.Authorization = `Bearer ${newToken}`;
                return API(error.config); // Retry request with new token
            } catch {
                store.dispatch({ type: "auth/logout" });
            }
        }
        return Promise.reject(error);
    }
);

export default API;
