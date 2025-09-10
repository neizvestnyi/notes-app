import React from 'react';
import {
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogContent,
  DialogBody,
  DialogActions,
  Button,
  Text
} from '@fluentui/react-components';
import type { Note } from '../types/Note';

interface DeleteConfirmDialogProps {
  open: boolean;
  note: Note | null;
  onConfirm: () => void;
  onCancel: () => void;
}

const DeleteConfirmDialog: React.FC<DeleteConfirmDialogProps> = ({ 
  open, 
  note, 
  onConfirm, 
  onCancel 
}) => {
  return (
    <Dialog open={open}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Delete Note</DialogTitle>
          <DialogContent>
            <Text>
              Are you sure you want to delete the note "{note?.title}"? 
              This action cannot be undone.
            </Text>
          </DialogContent>
          <DialogActions>
            <DialogTrigger disableButtonEnhancement>
              <Button appearance="secondary" onClick={onCancel}>
                Cancel
              </Button>
            </DialogTrigger>
            <Button appearance="primary" onClick={onConfirm}>
              Delete
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};

export default DeleteConfirmDialog;