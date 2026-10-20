import React, { useEffect, useState } from 'react';
import API from '../api/axios';
import logger from "../telemetry/logger";

// SS.User.API sends Role as the enum's number (SS.Base.Domain.Entities.Role).
const ROLE_NAMES = { 0: 'Admin', 1: 'Agent', 2: 'Customer' };

const UserList = () => {
  // State to store user data
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch user data from API when component mounts
  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const response = await API.get('/user/getallusers'); // Admin-only: [Authorize(Roles = "Admin")]
        setUsers(response.data); // [{ userId, name, email, role }]
        setLoading(false);
      } catch (err) {
        const status = err.response?.status;
        if (status === 403) {
          setError('Only administrators can view the user list.');
        } else if (status === 401) {
          setError('Your session has expired. Please sign in again.');
        } else {
          logger.error("Loading the user list failed", err);
          setError(`Could not load the user list: ${err.message}`);
        }
        setLoading(false);
      }
    };

    fetchUsers();
  }, []); // Empty array ensures it runs only once when the component mounts

  // Handle loading and error states
  if (loading) {
    return <p>Loading...</p>;
  }

  if (error) {
    return <p>{error}</p>;
  }

 const tableStyle = {
    width: '100%',
    borderCollapse: 'collapse',
    margin: '20px 0',
    fontSize: '18px',
    textAlign: 'left'
  };

  const thStyle = {
    backgroundColor: '#f2f2f2',
    borderBottom: '2px solid #dddddd',
    padding: '12px',
  };

  const tdStyle = {
    border: '1px solid #dddddd',
    padding: '12px',
    textAlign: 'left',
  };

  const trHoverStyle = {
    backgroundColor: '#f5f5f5',
  };

  // Render users in a styled table
  return (
    <div>
      <h1>Users List</h1>
      <table style={tableStyle}>
        <thead>
          <tr>
            <th style={thStyle}>Name</th>
            <th style={thStyle}>Email</th>
            <th style={thStyle}>Role</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user, index) => (
            <tr key={user.userId} style={index % 2 === 0 ? trHoverStyle : {}}>
              <td style={tdStyle}>{user.name}</td>
              <td style={tdStyle}>{user.email}</td>
              <td style={tdStyle}>{ROLE_NAMES[user.role] ?? user.role}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UserList;
