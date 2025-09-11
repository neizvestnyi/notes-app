using System.ComponentModel.DataAnnotations;

namespace NotesApp.Domain.DTOs;

public record NoteDto(
    Guid Id,
    string Title,
    string? Content,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public class CreateNoteDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(120, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 120 characters")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
    public string? Content { get; set; }
}

public class UpdateNoteDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(120, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 120 characters")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
    public string? Content { get; set; }
}