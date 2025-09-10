import React from 'react';
import { useMsal } from '@azure/msal-react';
import { Button } from '@fluentui/react-components';
import { loginRequest, apiConfig } from '../config/authConfig';

const LoginButton: React.FC = () => {
  const { instance, accounts, inProgress } = useMsal();

  const handleLogin = () => {
    if (apiConfig.useDevAuth) {
      // In development mode, we don't need actual login
      console.log('Development mode - authentication bypassed');
      return;
    }

    instance.loginPopup(loginRequest).catch((error) => {
      console.error('Login failed:', error);
    });
  };

  const handleLogout = () => {
    if (apiConfig.useDevAuth) {
      // In development mode, just refresh the page
      window.location.reload();
      return;
    }

    instance.logoutPopup().catch((error) => {
      console.error('Logout failed:', error);
    });
  };

  const isAuthenticated = apiConfig.useDevAuth || accounts.length > 0;

  return (
    <div style={{ 
      padding: '20px', 
      backgroundColor: '#fff', 
      borderBottom: '1px solid #e1e1e1',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }}>
      <h1>Notes App</h1>
      <div>
        {apiConfig.useDevAuth && (
          <span style={{ marginRight: '10px', color: '#666' }}>
            Development Mode
          </span>
        )}
        {isAuthenticated ? (
          <Button onClick={handleLogout} disabled={inProgress !== 'none'}>
            Sign Out
          </Button>
        ) : (
          <Button onClick={handleLogin} disabled={inProgress !== 'none'} appearance="primary">
            Sign In
          </Button>
        )}
      </div>
    </div>
  );
};

export default LoginButton;