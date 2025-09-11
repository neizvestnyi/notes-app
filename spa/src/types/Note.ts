export type Note = {
  id: string;
  title: string;
  content?: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type CreateNoteDto = {
  title: string;
  content?: string;
}

export type UpdateNoteDto = {
  title: string;
  content?: string;
}

export type ApiResponse<T> = {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[] | null;
  timestamp: string;
  traceId?: string;
}

export type PaginatedResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  search?: string | null;
  sortBy?: string | null;
  sortDescending: boolean;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  nextPage?: number | null;
  previousPage?: number | null;
  firstItemIndex: number;
  lastItemIndex: number;
}

export type NotesPagedRequest = {
  page?: number;
  pageSize?: number;
  search?: string;
  title?: string;
  content?: string;
  createdAfter?: string;
  createdBefore?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export type SortField = 'title' | 'content' | 'createdAtUtc' | 'updatedAtUtc';