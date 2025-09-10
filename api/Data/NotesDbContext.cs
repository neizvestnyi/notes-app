using Microsoft.EntityFrameworkCore;
using NotesApp.Api.Models;

namespace NotesApp.Api.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
    {
    }
    
    public DbSet<Note> Notes => Set<Note>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Content).HasMaxLength(5000);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.UpdatedAtUtc).IsRequired();
            
            entity.HasData(
                new Note
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    Title = "Welcome to Notes App",
                    Content = "This is your first note. Feel free to edit or delete it.",
                    CreatedAtUtc = new DateTime(2024, 9, 8, 12, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2024, 9, 8, 12, 0, 0, DateTimeKind.Utc)
                },
                new Note
                {
                    Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                    Title = "Project Ideas",
                    Content = "1. Build a task management system\n2. Create a recipe sharing platform\n3. Develop a fitness tracker",
                    CreatedAtUtc = new DateTime(2024, 9, 8, 12, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2024, 9, 8, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        });
    }
}