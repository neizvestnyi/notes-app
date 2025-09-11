using NotesApp.Domain.DTOs;

namespace NotesApp.Domain.Tests.DTOs;

public class UpdateNoteDtoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_WhenNullOrWhitespace_ShouldCreateDto(string? title)
    {
        var dto = new UpdateNoteDto { Title = title!, Content = "Content" };
        
        Assert.Equal(title, dto.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Content_WhenNullOrWhitespace_ShouldCreateDto(string? content)
    {
        var dto = new UpdateNoteDto { Title = "Title", Content = content! };
        
        Assert.Equal(content, dto.Content);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateDto()
    {
        var title = "Updated Title";
        var content = "Updated Content";
        
        var dto = new UpdateNoteDto { Title = title, Content = content };
        
        Assert.Equal(title, dto.Title);
        Assert.Equal(content, dto.Content);
    }
}