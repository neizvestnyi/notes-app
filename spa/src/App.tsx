import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import { msalConfig, apiConfig } from './config/authConfig';
import LoginButton from './components/LoginButton';
import NotesList from './components/NotesList';
import NotesService from './services/notesService';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';

const msalInstance = new PublicClientApplication(msalConfig);
const notesService = new NotesService(msalInstance);

function App() {
  // In development mode, show the NotesList directly
  if (apiConfig.useDevAuth) {
    return (
      <FluentProvider theme={webLightTheme}>
        <div style={{ minHeight: '100vh', backgroundColor: '#fafafa' }}>
          <LoginButton />
          <NotesList notesService={notesService} />
        </div>
      </FluentProvider>
    );
  }

  return (
    <MsalProvider instance={msalInstance}>
      <FluentProvider theme={webLightTheme}>
        <div style={{ minHeight: '100vh', backgroundColor: '#fafafa' }}>
          <LoginButton />
          <AuthenticatedTemplate>
            <NotesList notesService={notesService} />
          </AuthenticatedTemplate>
          <UnauthenticatedTemplate>
            <div style={{ 
              display: 'flex', 
              justifyContent: 'center', 
              alignItems: 'center', 
              height: '400px',
              flexDirection: 'column',
              gap: '20px'
            }}>
              <h2>Welcome to Notes App</h2>
              <p>Please sign in with your Azure AD account to manage your notes.</p>
            </div>
          </UnauthenticatedTemplate>
        </div>
      </FluentProvider>
    </MsalProvider>
  );
}

export default App;
