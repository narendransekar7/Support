import React from "react";
import { Link } from "react-router-dom";
import { useSelector } from "react-redux";
import { isAdmin } from "../auth/roles";

// Pages backed by Admin-only APIs (SS.User.API createuser / getallusers). The API enforces it; this only
// saves non-admins a form that can't succeed. Used inside ProtectedRoute, so the user is signed in.
const AdminRoute = ({ children }) => {
    const { userrole, profileStatus } = useSelector((state) => state.auth);

    if (profileStatus === "idle" || profileStatus === "loading") {
        return <p style={styles.message}>Loading...</p>;
    }

    if (!isAdmin(userrole)) {
        return (
            <div style={styles.message}>
                <p>Only administrators can manage users.</p>
                <Link to="/ticket/create">Go to Create Ticket</Link>
            </div>
        );
    }

    return children;
};

const styles = {
    message: { padding: 20, fontFamily: "Arial, sans-serif" },
};

export default AdminRoute;
