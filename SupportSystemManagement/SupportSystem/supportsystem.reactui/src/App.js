import React from 'react';
import { Provider } from "react-redux";
import store from "./app/store";
import AuthenticatedApp from "./AuthenticatedApp";






function App() {
   return (
    <Provider store={store}>
      <AuthenticatedApp />
    </Provider>
  );
}

export default App;
