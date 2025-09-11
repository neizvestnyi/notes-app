using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.Core.Models;
using NotesApp.Data.Entities;
using NotesApp.Data.Interfaces;
using NotesApp.Domain.DTOs;
using NotesApp.Domain.Services;

namespace NotesApp.Domain.Tests.Services;

public class NoteServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<NoteService>> _loggerMock;
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly NoteService _noteService;

    public NoteServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<NoteService>>();
        _noteRepositoryMock = new Mock<INoteRepository>();
        
        _unitOfWorkMock.Setup(x => x.Notes).Returns(_noteRepositoryMock.Object);
        
        _noteService = new NoteService(_unitOfWorkMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllNotesAsync_WhenCacheHit_ShouldReturnCachedNotes()
    {
        // Arrange
        var cachedNotes = new List<NoteDto>
        {
            new(Guid.NewGuid(), "Test Title", "Test Content", DateTime.UtcNow, DateTime.UtcNow)
        };
        
        object? cacheValue = cachedNotes;
        _cacheMock.Setup(x => x.TryGetValue("all_notes", out cacheValue)).Returns(true);

        // Act
        var result = await _noteService.GetAllNotesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Title", result.First().Title);
        _noteRepositoryMock.Verify(x => x.GetNotesOrderedByUpdatedDateAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllNotesAsync_WhenCacheMiss_ShouldFetchFromRepositoryAndCache()
    {
        // Arrange
        object? cacheValue = null;
        _cacheMock.Setup(x => x.TryGetValue("all_notes", out cacheValue)).Returns(false);
        
        var notes = new List<Note>
        {
            new("Test Title", "Test Content")
        };
        
        _noteRepositoryMock.Setup(x => x.GetNotesOrderedByUpdatedDateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes);

        var cacheEntry = new Mock<ICacheEntry>();
        _cacheMock.Setup(x => x.CreateEntry("all_notes")).Returns(cacheEntry.Object);

        // Act
        var result = await _noteService.GetAllNotesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Title", result.First().Title);
        _noteRepositoryMock.Verify(x => x.GetNotesOrderedByUpdatedDateAsync(It.IsAny<CancellationToken>()), Times.Once);
        cacheEntry.VerifySet(x => x.Value = It.IsAny<List<NoteDto>>(), Times.Once);
    }

    [Fact]
    public async Task GetNoteByIdAsync_WhenNoteExists_ShouldReturnNoteDto()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new Note("Test Title", "Test Content");
        
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);

        // Act
        var result = await _noteService.GetNoteByIdAsync(noteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Content", result.Content);
    }

    [Fact]
    public async Task GetNoteByIdAsync_WhenNoteDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _noteService.GetNoteByIdAsync(noteId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateNoteAsync_ShouldCreateNoteAndClearCache()
    {
        // Arrange
        var createDto = new CreateNoteDto { Title = "New Title", Content = "New Content" };
        
        _noteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note note, CancellationToken ct) => note);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _noteService.CreateNoteAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Content", result.Content);
        
        _noteRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.Remove("all_notes"), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_WhenNoteExists_ShouldUpdateAndClearCache()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new Note("Old Title", "Old Content");
        var updateDto = new UpdateNoteDto { Title = "Updated Title", Content = "Updated Content" };
        
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _noteService.UpdateNoteAsync(noteId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Content", result.Content);
        
        _noteRepositoryMock.Verify(x => x.Update(note), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.Remove("all_notes"), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_WhenNoteDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var updateDto = new UpdateNoteDto { Title = "Updated Title", Content = "Updated Content" };
        
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _noteService.UpdateNoteAsync(noteId, updateDto);

        // Assert
        Assert.Null(result);
        _noteRepositoryMock.Verify(x => x.Update(It.IsAny<Note>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteNoteAsync_WhenNoteExists_ShouldDeleteAndClearCache()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new Note("Title", "Content");
        
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _noteService.DeleteNoteAsync(noteId);

        // Assert
        Assert.True(result);
        _noteRepositoryMock.Verify(x => x.Delete(note), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.Remove("all_notes"), Times.Once);
    }

    [Fact]
    public async Task DeleteNoteAsync_WhenNoteDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _noteService.DeleteNoteAsync(noteId);

        // Assert
        Assert.False(result);
        _noteRepositoryMock.Verify(x => x.Delete(It.IsAny<Note>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchNotesAsync_ShouldReturnSearchResults()
    {
        // Arrange
        var searchTerm = "test";
        var notes = new List<Note>
        {
            new("Test Title", "Content"),
            new("Another Test", "More content")
        };
        
        _noteRepositoryMock.Setup(x => x.SearchByTitleAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notes);

        // Act
        var result = await _noteService.SearchNotesAsync(searchTerm);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, n => n.Title == "Test Title");
        Assert.Contains(result, n => n.Title == "Another Test");
    }

    [Fact]
    public async Task GetNotesPagedAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var request = new NotesPagedRequest { Page = 1, PageSize = 10, Search = "test" };
        var notes = new List<Note> { new("Test Title", "Content") };
        var paginatedResult = new PaginatedResponse<Note>
        {
            Items = notes,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            Search = "test"
        };
        
        _noteRepositoryMock.Setup(x => x.GetNotesPagedAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _noteService.GetNotesPagedAsync(request);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal("test", result.Search);
        Assert.Equal("Test Title", result.Items.First().Title);
    }
}