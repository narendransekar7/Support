import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import User from './components/User';
import Login from './components/Login';

function App() {
  return (
  
    <Router>
      <Routes>
        <Route path="/" element={<Login />}/>
		<Route path="user/adduser" element={<User />}/>
      </Routes>
    </Router>
  );
}

export default App;
