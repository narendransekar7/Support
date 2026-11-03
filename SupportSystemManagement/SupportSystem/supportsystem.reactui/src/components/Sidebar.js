import React, { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { useSelector } from "react-redux";
import { isAdmin } from "../auth/roles";

const Sidebar = () => {
    const [open, setOpen] = useState(true);
    const location = useLocation();
    const userrole = useSelector((state) => state.auth.userrole);

    // adminOnly: the page's API requires the Admin role, so other users don't see the link.
    const menuItems = [
        { path: "/user/list", label: "User List", adminOnly: true },
        { path: "/user/add", label: "Add User", adminOnly: true },
        { path: "/ticket/create", label: "Create Ticket" },
        { path: "/notifications", label: "Notifications" },
        { path: "/profile", label: "Profile" },
    ];

    const sidebarStyle = {
        backgroundColor: "#1E293B",
        color: "white",
        height: "100vh",
        width: open ? "200px" : "60px",
        transition: "width 0.3s",
        display: "flex",
        flexDirection: "column",
    };

    const headerStyle = {
        display: "flex",
        justifyContent: open ? "space-between" : "center",
        alignItems: "center",
        padding: "10px",
        borderBottom: "1px solid #334155",
    };

    const linkStyle = (active) => ({
        display: "block",
        padding: "10px 15px",
        margin: "4px 8px",
        borderRadius: "5px",
        backgroundColor: active ? "#3B82F6" : "transparent",
        color: active ? "#fff" : "#cbd5e1",
        textDecoration: "none",
        fontSize: "14px",
    });

    return (
        <div style={sidebarStyle}>
            {/* Header */}
            <div style={headerStyle}>
                {open && <span style={{ fontWeight: "bold" }}>Support</span>}
                <button
                    onClick={() => setOpen(!open)}
                    style={{
                        background: "none",
                        border: "none",
                        color: "#cbd5e1",
                        cursor: "pointer",
                        fontSize: "18px",
                    }}
                >
                    {open ? "✖" : "☰"}
                </button>
            </div>

            {/* Menu Items */}
            <nav style={{ marginTop: "10px", flex: 1 }}>
                {menuItems.filter((item) => !item.adminOnly || isAdmin(userrole)).map((item) => (
                    <Link
                        key={item.path}
                        to={item.path}
                        style={linkStyle(location.pathname === item.path)}
                    >
                        {open ? item.label : item.label.charAt(0)}
                    </Link>
                ))}
            </nav>
        </div>
    );
};

export default Sidebar;
