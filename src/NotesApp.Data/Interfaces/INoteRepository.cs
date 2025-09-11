using NotesApp.Data.Entities;

namespace NotesApp.Data.Interfaces;

public interface INoteRepository : IRepository<Note>
{
    Task<IEnumerable<Note>> GetNotesOrderedByUpdatedDateAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);
}