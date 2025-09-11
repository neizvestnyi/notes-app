using System.ComponentModel.DataAnnotations;
using NotesApp.Data.Common;

namespace NotesApp.Data.Entities;

public class Note : BaseEntity
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Title { get; private set; } = string.Empty;
    
    [StringLength(5000)]
    public string? Content { get; private set; }

    private Note() : base()
    {
    }

    public Note(string title, string? content = null) : base()
    {
        Title = title;
        Content = content;
    }

    public void UpdateNote(string title, string? content)
    {
        Title = title;
        Content = content;
        UpdateTimestamp();
    }
}