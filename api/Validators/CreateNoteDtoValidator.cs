using FluentValidation;
using NotesApp.Api.DTOs;

namespace NotesApp.Api.Validators;

public class CreateNoteDtoValidator : AbstractValidator<CreateNoteDto>
{
    public CreateNoteDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(120)
            .WithMessage("Title cannot exceed 120 characters.")
            .Must(NoteValidationHelpers.BeValidTitle)
            .WithMessage("Title cannot contain only whitespace.");

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters.")
            .Must(NoteValidationHelpers.BeValidContent)
            .WithMessage("Content cannot contain only whitespace when provided.");
    }
}

public class UpdateNoteDtoValidator : AbstractValidator<UpdateNoteDto>
{
    public UpdateNoteDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(120)
            .WithMessage("Title cannot exceed 120 characters.")
            .Must(NoteValidationHelpers.BeValidTitle)
            .WithMessage("Title cannot contain only whitespace.");

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters.")
            .Must(NoteValidationHelpers.BeValidContent)
            .WithMessage("Content cannot contain only whitespace when provided.");
    }
}