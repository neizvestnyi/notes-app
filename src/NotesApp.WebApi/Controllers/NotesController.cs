using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Domain.DTOs;
using NotesApp.Domain.Interfaces;
using NotesApp.Core.Models;

namespace NotesApp.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    /// <summary>
    /// Get all notes ordered by last updated date
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NoteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NoteDto>>>> GetAllNotes(CancellationToken cancellationToken)
    {
        var notes = await _noteService.GetAllNotesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<NoteDto>>.CreateSuccess(notes, $"Retrieved {notes.Count()} notes successfully"));
    }

    /// <summary>
    /// Get a specific note by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<NoteDto>>> GetNoteById(Guid id, CancellationToken cancellationToken)
    {
        var note = await _noteService.GetNoteByIdAsync(id, cancellationToken);
        
        if (note == null)
        {
            return NotFound(ApiResponse.CreateError("Note not found"));
        }

        return Ok(ApiResponse<NoteDto>.CreateSuccess(note, "Note retrieved successfully"));
    }

    /// <summary>
    /// Create a new note
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<NoteDto>>> CreateNote([FromBody] CreateNoteDto createNoteDto, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _noteService.CreateNoteAsync(createNoteDto, cancellationToken);
            return CreatedAtAction(
                nameof(GetNoteById), 
                new { id = note.Id }, 
                ApiResponse<NoteDto>.CreateSuccess(note, "Note created successfully")
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Update an existing note
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<NoteDto>>> UpdateNote(Guid id, [FromBody] UpdateNoteDto updateNoteDto, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _noteService.UpdateNoteAsync(id, updateNoteDto, cancellationToken);
            
            if (note == null)
            {
                return NotFound(ApiResponse.CreateError("Note not found"));
            }

            return Ok(ApiResponse<NoteDto>.CreateSuccess(note, "Note updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Delete a note
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> DeleteNote(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _noteService.DeleteNoteAsync(id, cancellationToken);
        
        if (!deleted)
        {
            return NotFound(ApiResponse.CreateError("Note not found"));
        }

        return Ok(ApiResponse.CreateSuccessMessage("Note deleted successfully"));
    }

    /// <summary>
    /// Search notes by title
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NoteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NoteDto>>>> SearchNotes([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(ApiResponse.CreateError("Search term is required"));
        }

        var notes = await _noteService.SearchNotesAsync(searchTerm, cancellationToken);
        return Ok(ApiResponse<IEnumerable<NoteDto>>.CreateSuccess(notes, $"Found {notes.Count()} notes matching '{searchTerm}'"));
    }
}