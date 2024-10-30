import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import User from './components/User';
import Login from './components/Login';
import UserList from './components/UserList';

function App() {
  return (
  
    <Router>
      <Routes>
        <Route path="/" element={<Login />}/>
		<Route path="user/add" element={<User />}/>
		<Route path="user/list" element={<UserList />}/>
      </Routes>
    </Router>
  );
}

export default App;
