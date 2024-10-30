import React, { useEffect, useState } from 'react';
import axios from 'axios';

const UserList = () => {
  // State to store user data
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch user data from API when component mounts
  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const response = await axios.get('https://localhost:7160/api/user/getallusers'); // Update with your correct API endpoint
		;debugger       
	   setUsers(response.data); // Set fetched data to state
        setLoading(false);
      } catch (err) {
        setError(err.message);
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
    return <p>Error: {error}</p>;
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
		  
		
            <th style={thStyle}>UserName</th>
            <th style={thStyle}>FirstName</th>
            <th style={thStyle}>LastName</th>
            <th style={thStyle}>DisplayName</th>
            <th style={thStyle}>PrimaryEmail</th>
            <th style={thStyle}>PrimaryPhoneNumber</th>
            <th style={thStyle}>Gender</th>
            <th style={thStyle}>Company</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user, index) => (
            <tr key={index} style={index % 2 === 0 ? trHoverStyle : {}}>
              <td style={tdStyle}>{user.userName}</td>
              <td style={tdStyle}>{user.firstName}</td>
              <td style={tdStyle}>{user.lastName}</td>
              <td style={tdStyle}>{user.displayName}</td>
              <td style={tdStyle}>{user.primaryEmail}</td>
              <td style={tdStyle}>{user.primaryPhoneNumber}</td>
              <td style={tdStyle}>{user.gender}</td>
              <td style={tdStyle}>{user.company}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UserList;
