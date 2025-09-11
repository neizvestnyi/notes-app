namespace NotesApp.Core.Models;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "UpdatedAtUtc";
    public bool SortDescending { get; set; } = true;

    public int Skip => (Page - 1) * PageSize;
    
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100;
    }
}

public class NotesPagedRequest : PagedRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}