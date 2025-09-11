import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { PublicClientApplication } from '@azure/msal-browser';
import type { Note, CreateNoteDto, UpdateNoteDto, ApiResponse, PaginatedResponse, NotesPagedRequest } from '../types/Note';
import { apiConfig, loginRequest } from '../config/authConfig';

export default class NotesService {
  private api: AxiosInstance;
  private msalInstance: PublicClientApplication;

  constructor(msalInstance: PublicClientApplication) {
    this.msalInstance = msalInstance;
    this.api = axios.create({
      baseURL: apiConfig.baseUrl,
    });

    this.api.interceptors.request.use(async (config) => {
      if (apiConfig.useDevAuth) {
        return config;
      }
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
    const response = await this.api.get<ApiResponse<Note[]>>('/api/v1/notes');
    return response.data.data;
  }

  async createNote(note: CreateNoteDto): Promise<Note> {
    const response = await this.api.post<ApiResponse<Note>>('/api/v1/notes', note);
    return response.data.data;
  }

  async updateNote(id: string, note: UpdateNoteDto): Promise<Note> {
    const response = await this.api.put<ApiResponse<Note>>(`/api/v1/notes/${id}`, note);
    return response.data.data;
  }

  async deleteNote(id: string): Promise<void> {
    await this.api.delete(`/api/v1/notes/${id}`);
  }

  async getNotesPagedAsync(request: NotesPagedRequest = {}): Promise<PaginatedResponse<Note>> {
    const params = new URLSearchParams();
    
    if (request.page !== undefined) params.append('page', request.page.toString());
    if (request.pageSize !== undefined) params.append('pageSize', request.pageSize.toString());
    if (request.search) params.append('search', request.search);
    if (request.title) params.append('title', request.title);
    if (request.content) params.append('content', request.content);
    if (request.createdAfter) params.append('createdAfter', request.createdAfter);
    if (request.createdBefore) params.append('createdBefore', request.createdBefore);
    if (request.sortBy) params.append('sortBy', request.sortBy);
    if (request.sortDescending !== undefined) params.append('sortDescending', request.sortDescending.toString());

    const response = await this.api.get<ApiResponse<PaginatedResponse<Note>>>(`/api/v1/notes/paged?${params}`);
    return response.data.data;
  }
}