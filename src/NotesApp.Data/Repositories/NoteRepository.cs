using Microsoft.EntityFrameworkCore;
using NotesApp.Data.Entities;
using NotesApp.Data.Interfaces;
using NotesApp.Core.Models;
using System.Linq.Expressions;

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

    public async Task<PaginatedResponse<Note>> GetNotesPagedAsync(NotesPagedRequest request, CancellationToken cancellationToken = default)
    {
        request.Validate();

        var query = _dbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(n => n.Title.ToLower().Contains(searchLower) || 
                                   n.Content.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var titleLower = request.Title.ToLower();
            query = query.Where(n => n.Title.ToLower().Contains(titleLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            var contentLower = request.Content.ToLower();
            query = query.Where(n => n.Content.ToLower().Contains(contentLower));
        }

        if (request.CreatedAfter.HasValue)
        {
            query = query.Where(n => n.CreatedAtUtc >= request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            query = query.Where(n => n.CreatedAtUtc <= request.CreatedBefore.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = ApplySorting(query, request.SortBy, request.SortDescending);
        }
        else
        {
            query = query.OrderByDescending(n => n.UpdatedAtUtc);
        }

        // Apply pagination
        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<Note>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };
    }

    private static IQueryable<Note> ApplySorting(IQueryable<Note> query, string sortBy, bool sortDescending)
    {
        Expression<Func<Note, object>> keySelector = sortBy.ToLower() switch
        {
            "title" => n => n.Title,
            "content" => n => n.Content,
            "createdatutc" => n => n.CreatedAtUtc,
            "updatedatutc" or _ => n => n.UpdatedAtUtc,
        };

        return sortDescending 
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}