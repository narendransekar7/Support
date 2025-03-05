import React, { useEffect } from "react";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { fetchUser } from "./features/authSlice";
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
	
	const token = localStorage.getItem("token");
	
	const response = await fetch(`http://localhost:5145/api/ticket/${params.id}`, {
    method: "GET",
    headers: {
      "Authorization": `Bearer ${token}` // Add Bearer token
      ,"Content-Type": "application/json"
    }
  });

  if (!response.ok) {
    throw new Response("Not Found", { status: 404 });
  }
  return response.json();
};
  
  
  
  const router = createBrowserRouter([
  { path: '/', element: <Login /> },
  { path: 'user/add', element: <User /> },
  { path: 'user/list', element: <UserList /> },
  { path: 'ticket/create', element: <TicketCreateForm /> },
  { path: 'ticket/:id', element: <TicketDetails />, loader: ticketLoader },
]);
  

  return <RouterProvider router={router} />;
};

export default AuthenticatedApp;
