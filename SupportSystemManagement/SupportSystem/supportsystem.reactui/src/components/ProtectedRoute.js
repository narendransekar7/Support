import React from "react";
import { Navigate } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { logoutUser } from "../features/authSlice";
import useSignedIn from "../auth/useSignedIn";

// Requires a session (password login or Microsoft Entra ID) and a matching Support System account.
const ProtectedRoute = ({ children }) => {
    const dispatch = useDispatch();
    const { signedIn, pending } = useSignedIn();
    const { profileStatus } = useSelector((state) => state.auth);

    if (!signedIn) {
        // While MSAL is still processing a redirect, don't bounce the user to the login page.
        return pending ? <p style={styles.message}>Signing in...</p> : <Navigate to="/" replace />;
    }

    if (profileStatus === "notRegistered") {
        return (
            <div style={styles.message}>
                <p>You are signed in, but your account isn't registered in the Support System. Ask an administrator to add you.</p>
                <button onClick={() => dispatch(logoutUser())}>Sign out</button>
            </div>
        );
    }

    return children;
};

const styles = {
    message: { padding: 20, fontFamily: "Arial, sans-serif" },
};

export default ProtectedRoute;
