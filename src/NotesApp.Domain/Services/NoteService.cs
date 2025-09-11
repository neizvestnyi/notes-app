using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotesApp.Domain.DTOs;
using NotesApp.Domain.Interfaces;
using NotesApp.Data.Entities;
using NotesApp.Data.Interfaces;
using NotesApp.Core.Models;

namespace NotesApp.Domain.Services;

public class NoteService : INoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NoteService> _logger;
    private const string AllNotesCacheKey = "all_notes";

    public NoteService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<NoteService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<NoteDto>> GetAllNotesAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<List<NoteDto>>(AllNotesCacheKey, out var cachedNotes))
        {
            _logger.LogInformation("Returning {Count} notes from cache", cachedNotes!.Count);
            return cachedNotes;
        }

        var notes = await _unitOfWork.Notes.GetNotesOrderedByUpdatedDateAsync(cancellationToken);
        var noteDtos = notes.Select(MapToDto).ToList();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        _cache.Set(AllNotesCacheKey, noteDtos, cacheOptions);
        _logger.LogInformation("Cached {Count} notes with 5 min sliding expiration", noteDtos.Count);

        return noteDtos;
    }

    public async Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(id, cancellationToken);
        return note == null ? null : MapToDto(note);
    }

    public async Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default)
    {
        var note = new Note(createNoteDto.Title, createNoteDto.Content);
        
        await _unitOfWork.Notes.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(AllNotesCacheKey);
        _logger.LogInformation("Created note with ID {NoteId} and cleared cache", note.Id);

        return MapToDto(note);
    }

    public async Task<NoteDto?> UpdateNoteAsync(Guid id, UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(id, cancellationToken);
        if (note == null)
            return null;

        note.UpdateNote(updateNoteDto.Title, updateNoteDto.Content);
        _unitOfWork.Notes.Update(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(AllNotesCacheKey);
        _logger.LogInformation("Updated note with ID {NoteId} and cleared cache", note.Id);

        return MapToDto(note);
    }

    public async Task<bool> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await _unitOfWork.Notes.GetByIdAsync(id, cancellationToken);
        if (note == null)
            return false;

        _unitOfWork.Notes.Delete(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(AllNotesCacheKey);
        _logger.LogInformation("Deleted note with ID {NoteId} and cleared cache", note.Id);

        return true;
    }

    public async Task<IEnumerable<NoteDto>> SearchNotesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var notes = await _unitOfWork.Notes.SearchByTitleAsync(searchTerm, cancellationToken);
        return notes.Select(MapToDto);
    }

    public async Task<PaginatedResponse<NoteDto>> GetNotesPagedAsync(NotesPagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Notes.GetNotesPagedAsync(request, cancellationToken);
        
        return new PaginatedResponse<NoteDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            Search = result.Search,
            SortBy = result.SortBy,
            SortDescending = result.SortDescending
        };
    }

    private static NoteDto MapToDto(Note note)
    {
        return new NoteDto(
            note.Id,
            note.Title,
            note.Content,
            note.CreatedAtUtc,
            note.UpdatedAtUtc
        );
    }
}