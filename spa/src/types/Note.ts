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