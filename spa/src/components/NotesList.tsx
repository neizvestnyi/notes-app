import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { 
  Button, 
  Card, 
  CardHeader, 
  Text,
  Body1,
  Caption1,
  Spinner
} from '@fluentui/react-components';
import { Add24Regular, Delete24Regular, Edit24Regular } from '@fluentui/react-icons';
import NotesService from '../services/notesService';
import type { Note, NotesPagedRequest, PaginatedResponse } from '../types/Note';
import NoteDialog from './NoteDialog';
import DeleteConfirmDialog from './DeleteConfirmDialog';
import SearchFilters from './SearchFilters';
import PaginationControls from './PaginationControls';

interface NotesListProps {
  notesService: NotesService;
}

const NotesList: React.FC<NotesListProps> = ({ notesService }) => {
  const [pagination, setPagination] = useState<PaginatedResponse<Note> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showNoteDialog, setShowNoteDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [selectedNote, setSelectedNote] = useState<Note | null>(null);
  const [filters, setFilters] = useState<NotesPagedRequest>({
    page: 1,
    pageSize: 10,
    sortBy: 'updatedAtUtc',
    sortDescending: true
  });

  const loadNotes = useCallback(async (resetToFirstPage = false) => {
    try {
      setLoading(true);
      const currentFilters = resetToFirstPage ? { ...filters, page: 1 } : filters;
      const paginatedData = await notesService.getNotesPagedAsync(currentFilters);
      setPagination(paginatedData);
      setError(null);
      
      if (resetToFirstPage) {
        setFilters(prev => ({ ...prev, page: 1 }));
      }
    } catch (err) {
      setError('Failed to load notes');
      console.error('Error loading notes:', err);
    } finally {
      setLoading(false);
    }
  }, [notesService, filters]);

  useEffect(() => {
    loadNotes();
  }, [loadNotes]);

  const handleFiltersChange = useCallback((newFilters: NotesPagedRequest) => {
    setFilters(newFilters);
  }, []);

  const handleSearch = useCallback(() => {
    loadNotes(true);
  }, [loadNotes]);

  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, []);

  const notes = useMemo(() => pagination?.items || [], [pagination]);

  const handleCreateNote = () => {
    setSelectedNote(null);
    setShowNoteDialog(true);
  };

  const handleEditNote = (note: Note) => {
    setSelectedNote(note);
    setShowNoteDialog(true);
  };

  const handleDeleteNote = (note: Note) => {
    setSelectedNote(note);
    setShowDeleteDialog(true);
  };

  const handleSaveNote = async (title: string, content: string) => {
    try {
      if (selectedNote) {
        await notesService.updateNote(selectedNote.id, { title, content });
      } else {
        await notesService.createNote({ title, content });
      }
      await loadNotes();
      setShowNoteDialog(false);
    } catch (err) {
      console.error('Error saving note:', err);
      setError('Failed to save note');
    }
  };

  const handleConfirmDelete = async () => {
    if (selectedNote) {
      try {
        await notesService.deleteNote(selectedNote.id);
        await loadNotes();
        setShowDeleteDialog(false);
        setSelectedNote(null);
      } catch (err) {
        console.error('Error deleting note:', err);
        setError('Failed to delete note');
      }
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <Text size={600}>
          Your Notes {pagination && `(${pagination.totalCount})`}
        </Text>
        <Button 
          icon={<Add24Regular />} 
          onClick={handleCreateNote}
          appearance="primary"
        >
          New Note
        </Button>
      </div>

      <SearchFilters
        filters={filters}
        onFiltersChange={handleFiltersChange}
        onSearch={handleSearch}
        loading={loading}
      />

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: '40px' }}>
          <Spinner label="Loading notes..." />
        </div>
      ) : error ? (
        <div style={{ padding: '20px', textAlign: 'center' }}>
          <Text style={{ color: 'red' }}>{error}</Text>
          <br />
          <Button onClick={() => loadNotes()} style={{ marginTop: '10px' }}>
            Retry
          </Button>
        </div>
      ) : notes.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
          <Text>
            {pagination?.search || filters.title || filters.content 
              ? 'No notes match your search criteria.' 
              : 'No notes yet. Create your first note!'}
          </Text>
        </div>
      ) : (
        <>
          <div style={{ 
            display: 'grid', 
            gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', 
            gap: '16px',
            marginBottom: '20px'
          }}>
            {notes.map((note) => (
              <Card key={note.id}>
                <CardHeader
                  header={<Body1>{note.title}</Body1>}
                  description={
                    <Caption1>
                      {new Date(note.updatedAtUtc).toLocaleDateString()}
                    </Caption1>
                  }
                  action={
                    <div style={{ display: 'flex', gap: '8px' }}>
                      <Button 
                        icon={<Edit24Regular />} 
                        onClick={() => handleEditNote(note)}
                        size="small"
                      />
                      <Button 
                        icon={<Delete24Regular />} 
                        onClick={() => handleDeleteNote(note)}
                        size="small"
                      />
                    </div>
                  }
                />
                {note.content && (
                  <div style={{ padding: '12px' }}>
                    <Text size={200} style={{ whiteSpace: 'pre-wrap' }}>
                      {note.content.length > 150 
                        ? `${note.content.substring(0, 150)}...` 
                        : note.content
                      }
                    </Text>
                  </div>
                )}
              </Card>
            ))}
          </div>

          {pagination && (
            <PaginationControls
              pagination={pagination}
              onPageChange={handlePageChange}
              loading={loading}
            />
          )}
        </>
      )}

      <NoteDialog
        open={showNoteDialog}
        note={selectedNote}
        onSave={handleSaveNote}
        onCancel={() => setShowNoteDialog(false)}
      />

      <DeleteConfirmDialog
        open={showDeleteDialog}
        note={selectedNote}
        onConfirm={handleConfirmDelete}
        onCancel={() => setShowDeleteDialog(false)}
      />
    </div>
  );
};

export default NotesList;