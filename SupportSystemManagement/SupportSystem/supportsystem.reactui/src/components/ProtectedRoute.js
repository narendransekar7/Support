//import { Navigate, Outlet } from "react-router-dom";
//import { useSelector } from "react-redux";

//const ProtectedRoute = () => {
//  const { token } = useSelector((state) => state.auth);
//    debugger;
//  return token ? <Outlet /> : <Navigate to="/login" />;
//};

//export default ProtectedRoute;

import React from "react";
import { Navigate } from "react-router-dom";
import { useSelector } from "react-redux";

const ProtectedRoute = ({ children }) => {
    const { token } = useSelector((state) => state.auth);
    return token ? children : <Navigate to="/" replace />;
};

export default ProtectedRoute;