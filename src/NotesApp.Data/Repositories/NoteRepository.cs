using Microsoft.EntityFrameworkCore;
using NotesApp.Data.Entities;
using NotesApp.Data.Interfaces;

namespace NotesApp.Data.Repositories;

public class NoteRepository : Repository<Note>, INoteRepository
{
    public NoteRepository(NotesDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Note>> GetNotesOrderedByUpdatedDateAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(n => n.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.Title.Contains(searchTerm))
            .OrderByDescending(n => n.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}