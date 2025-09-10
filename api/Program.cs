using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

// Health check endpoint (no auth required)
var useDevAuth = builder.Configuration.GetValue<bool>("UseDevAuthentication", false);
app.MapGet("/health", () => 
{
    var healthData = new { 
        status = "healthy", 
        timestamp = DateTime.UtcNow, 
        environment = app.Environment.EnvironmentName,
        authMode = useDevAuth ? "development" : "production" 
    };
    return healthData.ToApiResponse("System is healthy");
})
.WithName("HealthCheck")
.WithOpenApi()
.WithSummary("Health check endpoint")
.WithDescription("Returns the current health status of the API");

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

// GET /api/notes - Get all notes
app.MapGet("/api/notes", async (NotesDbContext db) =>
{
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
    
    return notes.ToApiResponse($"Retrieved {notes.Count} notes successfully");
})
.RequireAuthorization()
.WithName("GetNotes")
.WithOpenApi()
.WithSummary("Get all notes")
.WithDescription("Retrieves all notes for the authenticated user, ordered by last updated date")
.Produces<ApiResponse<List<NoteDto>>>(200)
.Produces<ApiResponse>(401);

// GET /api/notes/{id} - Get note by ID
app.MapGet("/api/notes/{id}", async (Guid id, NotesDbContext db) =>
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

// POST /api/notes - Create new note
app.MapPost("/api/notes", async (CreateNoteDto dto, NotesDbContext db, IValidator<CreateNoteDto> validator) =>
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
    
    var responseDto = new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    };
    
    return responseDto.ToCreatedApiResponse($"/api/notes/{note.Id}", "Note created successfully");
})
.RequireAuthorization()
.WithName("CreateNote")
.WithOpenApi()
.WithSummary("Create new note")
.WithDescription("Creates a new note with the provided title and content")
.Produces<ApiResponse<NoteDto>>(201)
.Produces<ApiResponse>(400)
.Produces<ApiResponse>(401);

// PUT /api/notes/{id} - Update existing note
app.MapPut("/api/notes/{id}", async (Guid id, UpdateNoteDto dto, NotesDbContext db, IValidator<UpdateNoteDto> validator) =>
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

// DELETE /api/notes/{id} - Delete note
app.MapDelete("/api/notes/{id}", async (Guid id, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        throw new NotFoundException("Note", id);
    
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    
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
