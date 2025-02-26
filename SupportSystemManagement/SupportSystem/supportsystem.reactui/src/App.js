import React from 'react';
import {
  createBrowserRouter,
  RouterProvider,
} from 'react-router-dom';
import User from './components/User';
import Login from './components/Login';
import UserList from './components/UserList';
import TicketCreateForm from './components/TicketCreateForm';
import TicketDetails from './components/TicketDetails';

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

function App() {
  return <RouterProvider router={router} />;
}

export default App;
