import React from 'react';
import { Provider } from "react-redux";
import { MsalProvider } from "@azure/msal-react";
import store from "./app/store";
import { msalInstance } from "./auth/authConfig";
import AuthenticatedApp from "./AuthenticatedApp";


function App() {
   return (
    <MsalProvider instance={msalInstance}>
      <Provider store={store}>
        <AuthenticatedApp />
      </Provider>
    </MsalProvider>
  );
}

export default App;
