using NotesApp.Data.Entities;
using NotesApp.Core.Models;

namespace NotesApp.Data.Interfaces;

public interface INoteRepository : IRepository<Note>
{
    Task<IEnumerable<Note>> GetNotesOrderedByUpdatedDateAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Note>> GetNotesPagedAsync(NotesPagedRequest request, CancellationToken cancellationToken = default);
}