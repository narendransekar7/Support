import React from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import Sidebar from "./Sidebar";
import { logoutUser } from "../features/authSlice";

const MasterLayout = () => {
   const dispatch = useDispatch();
   const navigate = useNavigate();

   const handleLogout = () => {
       dispatch(logoutUser());
       localStorage.removeItem("token");
       navigate("/");
   };

   const layoutStyle = {
       display: "flex",
       height: "100vh",
       backgroundColor: "#f1f5f9",
       fontFamily: "Arial, sans-serif",
   };

   const mainContainerStyle = {
       flex: 1,
       display: "flex",
       flexDirection: "column",
   };

   const headerStyle = {
       backgroundColor: "#ffffff",
       padding: "10px 20px",
       display: "flex",
       justifyContent: "space-between",
       alignItems: "center",
       borderBottom: "1px solid #e2e8f0",
       boxShadow: "0 2px 4px rgba(0,0,0,0.05)",
   };

   const titleStyle = {
       fontSize: "18px",
       fontWeight: "bold",
       color: "#1e293b",
       textAlign: "center",
       flex: 1,
   };

   const contentStyle = {
       flex: 1,
       padding: "20px",
       overflowY: "auto",
   };

   const buttonStyle = {
       backgroundColor: "#ef4444",
       color: "#fff",
       border: "none",
       padding: "6px 12px",
       borderRadius: "4px",
       cursor: "pointer",
   };

   return (
       <div style={layoutStyle}>
           {/* Sidebar */}
           <Sidebar />

           {/* Main Area */}
           <div style={mainContainerStyle}>
               {/* Header */}
               <header style={headerStyle}>
                   <h1 style={titleStyle}>
                       Support System
                   </h1>
                   <button onClick={handleLogout} style={buttonStyle}>
                       Logout
                   </button>
               </header>

               {/* Page Content */}
               <main style={contentStyle}>
                   <Outlet />
               </main>
           </div>
       </div>
   );
};

export default MasterLayout;
