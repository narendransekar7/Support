import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

function Login() {
  // State to hold form inputs
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();
  // const gatewayUrl = process.env.REACT_APP_GATEWAY_URL;
		  // console.log('Gateway URL:', gatewayUrl);
  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
	   try {
		   
		  
      const response = await axios.post('http://localhost:5145/api/auth/login', {
        email,
        password,
      });
debugger;
      // Save JWT token to localStorage or cookies
      localStorage.setItem('token', response.data.token);
	  localStorage.setItem("refreshToken", response.data.refreshToken);
      localStorage.setItem("email", email);
	  navigate('user/add');
      //alert('Login Successful');
    } catch (err) {
      //setError('Invalid Credentials');
    }
	
	
	
    // For now, just log the email and password, you can send these to an API
    //console.log("Email:", email);
    //console.log("Password:", password);

    // Clear the form (optional)
    //setEmail('');
    //setPassword('');
  };

  return (
    <div style={styles.container}>
      <h2>Support System Login</h2>
      <form onSubmit={handleSubmit} style={styles.form}>
        <div style={styles.inputContainer}>
          <label htmlFor="email">Email:</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            style={styles.input}
          />
        </div>
        <div style={styles.inputContainer}>
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            style={styles.input}
          />
        </div>
        <button type="submit" style={styles.button}>Login</button>
      </form>
    </div>
  );
}

const styles = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh',
    backgroundColor: '#f0f0f0',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    width: '300px',
    padding: '20px',
    backgroundColor: '#fff',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px',
  },
  inputContainer: {
    marginBottom: '15px',
  },
  input: {
    width: '100%',
    padding: '10px',
    borderRadius: '4px',
    border: '1px solid #ccc',
  },
  button: {
    padding: '10px',
    backgroundColor: '#007bff',
    color: '#fff',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
  },
};

export default Login;
