namespace NotesApp.WebApi.Validators;

public static class NoteValidationHelpers
{
    public static bool BeValidTitle(string? title)
    {
        return !string.IsNullOrWhiteSpace(title);
    }

    public static bool BeValidContent(string? content)
    {
        return content == null || !string.IsNullOrWhiteSpace(content);
    }
}