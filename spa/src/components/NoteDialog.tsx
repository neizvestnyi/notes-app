import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogContent,
  DialogBody,
  DialogActions,
  Button,
  Input,
  Textarea,
  Field
} from '@fluentui/react-components';
import type { Note } from '../types/Note';

interface NoteDialogProps {
  open: boolean;
  note: Note | null;
  onSave: (title: string, content: string) => void;
  onCancel: () => void;
}

const NoteDialog: React.FC<NoteDialogProps> = ({ open, note, onSave, onCancel }) => {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');

  useEffect(() => {
    if (note) {
      setTitle(note.title);
      setContent(note.content || '');
    } else {
      setTitle('');
      setContent('');
    }
  }, [note, open]);

  const handleSave = () => {
    if (title.trim()) {
      onSave(title.trim(), content.trim());
    }
  };

  const handleCancel = () => {
    setTitle('');
    setContent('');
    onCancel();
  };

  return (
    <Dialog open={open}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>{note ? 'Edit Note' : 'Create New Note'}</DialogTitle>
          <DialogContent style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <Field label="Title" required>
              <Input
                value={title}
                onChange={(_, data) => setTitle(data.value)}
                placeholder="Enter note title"
                maxLength={120}
              />
            </Field>
            <Field label="Content">
              <Textarea
                value={content}
                onChange={(_, data) => setContent(data.value)}
                placeholder="Enter note content (optional)"
                resize="vertical"
                rows={6}
                maxLength={5000}
              />
            </Field>
          </DialogContent>
          <DialogActions>
            <DialogTrigger disableButtonEnhancement>
              <Button appearance="secondary" onClick={handleCancel}>
                Cancel
              </Button>
            </DialogTrigger>
            <Button 
              appearance="primary" 
              onClick={handleSave}
              disabled={!title.trim()}
            >
              {note ? 'Update' : 'Create'}
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};

export default NoteDialog;