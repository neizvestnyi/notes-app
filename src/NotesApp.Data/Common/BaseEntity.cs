namespace NotesApp.Data.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateTimestamp()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}