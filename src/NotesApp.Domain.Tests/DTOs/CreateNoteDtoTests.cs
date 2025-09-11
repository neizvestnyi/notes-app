using NotesApp.Domain.DTOs;

namespace NotesApp.Domain.Tests.DTOs;

public class CreateNoteDtoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_WhenNullOrWhitespace_ShouldCreateDto(string? title)
    {
        var dto = new CreateNoteDto { Title = title!, Content = "Content" };
        
        Assert.Equal(title, dto.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Content_WhenNullOrWhitespace_ShouldCreateDto(string? content)
    {
        var dto = new CreateNoteDto { Title = "Title", Content = content! };
        
        Assert.Equal(content, dto.Content);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateDto()
    {
        var title = "Test Title";
        var content = "Test Content";
        
        var dto = new CreateNoteDto { Title = title, Content = content };
        
        Assert.Equal(title, dto.Title);
        Assert.Equal(content, dto.Content);
    }
}