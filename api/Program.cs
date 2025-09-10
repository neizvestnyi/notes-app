using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NotesApp.Api.Data;
using NotesApp.Api.DTOs;
using NotesApp.Api.Exceptions;
using NotesApp.Api.Extensions;
using NotesApp.Api.Middleware;
using NotesApp.Api.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services using extension methods
builder.Services.AddAuthentication(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddValidation();
builder.Services.AddCustomCors();
builder.Services.AddApiDocumentation();

// Add HTTP context accessor for logging
builder.Services.AddHttpContextAccessor();

// Add memory caching
builder.Services.AddMemoryCache();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotesDbContext>("database");

var app = builder.Build();

ResultExtensions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowSPA");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

var useDevAuth = builder.Configuration.GetValue<bool>("UseDevAuthentication", false);
app.MapGet("/health/detailed", () => 
{
    var healthData = new { 
        status = "healthy", 
        timestamp = DateTime.UtcNow, 
        environment = app.Environment.EnvironmentName,
        authMode = useDevAuth ? "development" : "production",
        version = "1.0.0"
    };
    return healthData.ToApiResponse("System is healthy");
})
.WithName("DetailedHealthCheck")
.WithOpenApi()
.WithSummary("Detailed health check")
.WithDescription("Returns detailed health status including version and environment info");

// Auth info endpoint for debugging (no auth required in dev)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/auth-info", (HttpContext context) => 
    {
        var authInfo = new { 
            authenticationMode = useDevAuth ? "development" : "azuread",
            isAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
            userName = context.User.Identity?.Name,
            claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
        return authInfo.ToApiResponse("Authentication information retrieved");
    })
    .WithName("AuthInfo")
    .WithOpenApi()
    .WithSummary("Authentication debug info")
    .WithDescription("Development-only endpoint for debugging authentication");
}

var v1 = app.MapGroup("/api/v1");

v1.MapGet("/notes", async (NotesDbContext db, IMemoryCache cache, ILogger<Program> logger) =>
{
    const string cacheKey = "all_notes";
    
    if (cache.TryGetValue<List<NoteDto>>(cacheKey, out var cachedNotes))
    {
        logger.LogInformation("Returning {Count} notes from cache", cachedNotes!.Count);
        return cachedNotes.ToApiResponse($"Retrieved {cachedNotes.Count} notes successfully (cached)");
    }
    
    var notes = await db.Notes
        .OrderByDescending(n => n.UpdatedAtUtc)
        .Select(n => new NoteDto
        {
            Id = n.Id,
            Title = n.Title,
            Content = n.Content,
            CreatedAtUtc = n.CreatedAtUtc,
            UpdatedAtUtc = n.UpdatedAtUtc
        })
        .ToListAsync();
    
    var cacheOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
    
    cache.Set(cacheKey, notes, cacheOptions);
    logger.LogInformation("Cached {Count} notes with 5 min sliding expiration", notes.Count);
    
    return notes.ToApiResponse($"Retrieved {notes.Count} notes successfully");
})
.RequireAuthorization()
.WithName("GetNotes")
.WithOpenApi()
.WithSummary("Get all notes")
.WithDescription("Retrieves all notes for the authenticated user, ordered by last updated date")
.Produces<ApiResponse<List<NoteDto>>>(200)
.Produces<ApiResponse>(401);

v1.MapGet("/notes/{id}", async (Guid id, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        throw new NotFoundException("Note", id);
    
    var noteDto = new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    };
    
    return noteDto.ToApiResponse("Note retrieved successfully");
})
.RequireAuthorization()
.WithName("GetNoteById")
.WithOpenApi()
.WithSummary("Get note by ID")
.WithDescription("Retrieves a specific note by its unique identifier")
.Produces<ApiResponse<NoteDto>>(200)
.Produces<ApiResponse>(404)
.Produces<ApiResponse>(401);

v1.MapPost("/notes", async (CreateNoteDto dto, NotesDbContext db, IValidator<CreateNoteDto> validator, IMemoryCache cache) =>
{
    // Validate input
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        throw new NotesApp.Api.Exceptions.ValidationException(errors);
    }
    
    var note = new Note
    {
        Id = Guid.NewGuid(),
        Title = dto.Title.Trim(),
        Content = dto.Content?.Trim(),
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };
    
    db.Notes.Add(note);
    await db.SaveChangesAsync();
    
    // Invalidate cache after creating note
    cache.Remove("all_notes");
    
    var responseDto = new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    };
    
    return responseDto.ToCreatedApiResponse($"/api/v1/notes/{note.Id}", "Note created successfully");
})
.RequireAuthorization()
.WithName("CreateNote")
.WithOpenApi()
.WithSummary("Create new note")
.WithDescription("Creates a new note with the provided title and content")
.Produces<ApiResponse<NoteDto>>(201)
.Produces<ApiResponse>(400)
.Produces<ApiResponse>(401);

v1.MapPut("/notes/{id}", async (Guid id, UpdateNoteDto dto, NotesDbContext db, IValidator<UpdateNoteDto> validator, IMemoryCache cache) =>
{
    // Validate input
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        throw new NotesApp.Api.Exceptions.ValidationException(errors);
    }
    
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        throw new NotFoundException("Note", id);
    
    note.Title = dto.Title.Trim();
    note.Content = dto.Content?.Trim();
    note.UpdatedAtUtc = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    // Invalidate cache after updating note
    cache.Remove("all_notes");
    
    var responseDto = new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    };
    
    return responseDto.ToApiResponse("Note updated successfully");
})
.RequireAuthorization()
.WithName("UpdateNote")
.WithOpenApi()
.WithSummary("Update existing note")
.WithDescription("Updates an existing note with the provided title and content")
.Produces<ApiResponse<NoteDto>>(200)
.Produces<ApiResponse>(400)
.Produces<ApiResponse>(404)
.Produces<ApiResponse>(401);

v1.MapDelete("/notes/{id}", async (Guid id, NotesDbContext db, IMemoryCache cache) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        throw new NotFoundException("Note", id);
    
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    
    // Invalidate cache after deleting note
    cache.Remove("all_notes");
    
    return ResultExtensions.ToNoContentApiResponse("Note deleted successfully");
})
.RequireAuthorization()
.WithName("DeleteNote")
.WithOpenApi()
.WithSummary("Delete note")
.WithDescription("Deletes an existing note by its unique identifier")
.Produces<ApiResponse>(200)
.Produces<ApiResponse>(404)
.Produces<ApiResponse>(401);

try
{
    Log.Information("Starting Notes API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
