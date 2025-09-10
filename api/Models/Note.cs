using System.ComponentModel.DataAnnotations;

namespace NotesApp.Api.Models;

public class Note
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(5000)]
    public string? Content { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime UpdatedAtUtc { get; set; }
}