import type { Configuration } from '@azure/msal-browser';

const useDevAuth = import.meta.env.VITE_USE_DEV_AUTH === 'true';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AAD_CLIENT_ID || 'dev-client-id',
    authority: import.meta.env.VITE_AAD_AUTHORITY || 'https://login.microsoftonline.com/common',
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: [import.meta.env.VITE_API_SCOPE || 'api://dev-api/.default'],
};

export const apiConfig = {
  baseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5209',
  useDevAuth,
};