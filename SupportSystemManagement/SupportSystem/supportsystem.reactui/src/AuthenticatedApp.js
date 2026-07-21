import React, { useEffect } from "react";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { fetchUser } from "./features/authSlice";
import API from "./api/axios";
import MasterLayout from "./components/MasterLayout";
import ProtectedRoute from "./components/ProtectedRoute";
import Login from './components/Login';
import User from './components/User';
import UserList from './components/UserList';
import TicketCreateForm from './components/TicketCreateForm';
import TicketDetails from './components/TicketDetails';

const AuthenticatedApp = () => {
  const dispatch = useDispatch();
  const { token } = useSelector((state) => state.auth);

  useEffect(() => {
    if (token) {
      dispatch(fetchUser());
    }
  }, [dispatch, token]);
/*
  const router = createBrowserRouter([
    { path: "/login", element: <Login /> },
    {
      path: "/dashboard",
      element: <ProtectedRoute />, // Protects Dashboard
      children: [{ path: "", element: <Dashboard /> }],
    },
  ]);
  
  */
  // Loader function to fetch ticket details
const ticketLoader = async ({ params }) => {
  try {
    const response = await API.get(`/ticket/${params.id}`);
    return response.data;
  } catch (error) {
    throw new Response("Not Found", { status: 404 });
  }
};
  
  
  
//  const router = createBrowserRouter([
//  { path: '/', element: <Login /> },
//  { path: 'user/add', element: <User /> },
//  { path: 'user/list', element: <UserList /> },
//  { path: 'ticket/create', element: <TicketCreateForm /> },
//  { path: 'ticket/:id', element: <TicketDetails />, loader: ticketLoader },
//]);
    const router = createBrowserRouter([
        { path: "/", element: <Login /> },
        {
            path: "/",
            element: <ProtectedRoute><MasterLayout /></ProtectedRoute>,
            children: [
                { path: "user/add", element: <User /> },
                { path: "user/list", element: <UserList /> },
                { path: "ticket/create", element: <TicketCreateForm /> },
                { path: "ticket/:id", element: <TicketDetails />, loader: ticketLoader },
            ],
        },
    ]);


  return <RouterProvider router={router} />;
};

export default AuthenticatedApp;
