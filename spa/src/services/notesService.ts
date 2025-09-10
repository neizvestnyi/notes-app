import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { PublicClientApplication } from '@azure/msal-browser';
import type { Note, CreateNoteDto, UpdateNoteDto, ApiResponse } from '../types/Note';
import { apiConfig, loginRequest } from '../config/authConfig';

export default class NotesService {
  private api: AxiosInstance;
  private msalInstance: PublicClientApplication;

  constructor(msalInstance: PublicClientApplication) {
    this.msalInstance = msalInstance;
    this.api = axios.create({
      baseURL: apiConfig.baseUrl,
    });

    // Add request interceptor for authentication
    this.api.interceptors.request.use(async (config) => {
      if (apiConfig.useDevAuth) {
        // Development authentication - no token needed
        return config;
      }

      // Production authentication
      try {
        const accounts = this.msalInstance.getAllAccounts();
        if (accounts.length > 0) {
          const response = await this.msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: accounts[0],
          });
          config.headers.Authorization = `Bearer ${response.accessToken}`;
        }
      } catch (error) {
        console.error('Failed to acquire token:', error);
      }
      
      return config;
    });
  }

  async getAllNotes(): Promise<Note[]> {
    const response = await this.api.get<ApiResponse<Note[]>>('/api/notes');
    return response.data.data;
  }

  async createNote(note: CreateNoteDto): Promise<Note> {
    const response = await this.api.post<ApiResponse<Note>>('/api/notes', note);
    return response.data.data;
  }

  async updateNote(id: string, note: UpdateNoteDto): Promise<Note> {
    const response = await this.api.put<ApiResponse<Note>>(`/api/notes/${id}`, note);
    return response.data.data;
  }

  async deleteNote(id: string): Promise<void> {
    await this.api.delete(`/api/notes/${id}`);
  }
}