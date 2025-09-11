using NotesApp.Domain.DTOs;

namespace NotesApp.Domain.Interfaces;

public interface INoteService
{
    Task<IEnumerable<NoteDto>> GetAllNotesAsync(CancellationToken cancellationToken = default);
    Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default);
    Task<NoteDto?> UpdateNoteAsync(Guid id, UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NoteDto>> SearchNotesAsync(string searchTerm, CancellationToken cancellationToken = default);
}