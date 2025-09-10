using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using NotesApp.Api.Data;
using NotesApp.Api.DTOs;
using NotesApp.Api.Models;
using NotesApp.Api.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configuration for authentication mode
var useDevAuth = builder.Configuration.GetValue<bool>("UseDevAuthentication", false);
var authScheme = useDevAuth ? "DevAuth" : JwtBearerDefaults.AuthenticationScheme;

if (useDevAuth && builder.Environment.IsDevelopment())
{
    // Development authentication - simple scheme for testing
    builder.Services.AddAuthentication(authScheme)
        .AddScheme<DevAuthenticationSchemeOptions, DevAuthenticationHandler>(
            "DevAuth", 
            "Development Authentication", 
            options => 
            {
                options.DefaultUserId = "dev-user-123";
                options.DefaultUserName = "Development User";
                options.DefaultUserEmail = "developer@notesapp.local";
            });
    
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("DevAuth")
            .Build();
    });
}
else
{
    // Production Azure AD authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    
    builder.Services.AddAuthorization();
}

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSPA");
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint (no auth required)
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow, authMode = useDevAuth ? "development" : "production" })
    .WithName("HealthCheck")
    .WithOpenApi();

// Auth info endpoint for debugging (no auth required in dev)
if (builder.Environment.IsDevelopment())
{
    app.MapGet("/api/auth-info", (HttpContext context) => 
    {
        return new { 
            authenticationMode = useDevAuth ? "development" : "azuread",
            isAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
            userName = context.User.Identity?.Name,
            claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
    })
    .WithName("AuthInfo")
    .WithOpenApi();
}

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
    
    return Results.Ok(notes);
})
.RequireAuthorization()
.WithName("GetNotes")
.WithOpenApi();

app.MapGet("/api/notes/{id}", async (Guid id, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        return Results.NotFound(new { message = "Note not found" });
    
    return Results.Ok(new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    });
})
.RequireAuthorization()
.WithName("GetNoteById")
.WithOpenApi();

app.MapPost("/api/notes", async (CreateNoteDto dto, NotesDbContext db, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { message = "Title is required" });
    
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
    
    return Results.Created($"/api/notes/{note.Id}", responseDto);
})
.RequireAuthorization()
.WithName("CreateNote")
.WithOpenApi();

app.MapPut("/api/notes/{id}", async (Guid id, UpdateNoteDto dto, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        return Results.NotFound(new { message = "Note not found" });
    
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { message = "Title is required" });
    
    note.Title = dto.Title.Trim();
    note.Content = dto.Content?.Trim();
    note.UpdatedAtUtc = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    return Results.Ok(new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedAtUtc = note.CreatedAtUtc,
        UpdatedAtUtc = note.UpdatedAtUtc
    });
})
.RequireAuthorization()
.WithName("UpdateNote")
.WithOpenApi();

app.MapDelete("/api/notes/{id}", async (Guid id, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    
    if (note == null)
        return Results.NotFound(new { message = "Note not found" });
    
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
})
.RequireAuthorization()
.WithName("DeleteNote")
.WithOpenApi();

app.Run();
