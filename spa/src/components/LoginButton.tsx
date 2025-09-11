import React from 'react';
import { useMsal } from '@azure/msal-react';
import { Button } from '@fluentui/react-components';
import { loginRequest, apiConfig } from '../config/authConfig';
import styles from './LoginButton.module.css';


const LoginButton: React.FC = () => {
  const { instance, accounts, inProgress } = useMsal();

  const handleLogin = () => {
    if (apiConfig.useDevAuth) {
      console.log('Development mode - authentication bypassed');
      return;
    }

    instance.loginPopup(loginRequest).catch((error) => {
      console.error('Login failed:', error);
    });
  };

  const handleLogout = () => {
    if (apiConfig.useDevAuth) {
      window.location.reload();
      return;
    }

    instance.logoutPopup().catch((error) => {
      console.error('Logout failed:', error);
    });
  };

  const isAuthenticated = apiConfig.useDevAuth || accounts.length > 0;

  return (
    <div className={styles.header}>
      <h1>Notes App</h1>
      <div>
        {apiConfig.useDevAuth && (
          <span className={styles.devModeLabel}>
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