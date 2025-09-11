namespace NotesApp.Api.Exceptions;

public abstract class NotesAppException : Exception
{
    public abstract int StatusCode { get; }
    
    protected NotesAppException(string message) : base(message) { }
    
    protected NotesAppException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotFoundException : NotesAppException
{
    public override int StatusCode => 404;
    
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} with id '{id}' was not found.") { }
}

public class ValidationException : NotesAppException
{
    public override int StatusCode => 400;
    public List<string> Errors { get; }
    
    public ValidationException(string message) : base(message)
    {
        Errors = [message];
    }
    
    public ValidationException(List<string> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public class ConflictException : NotesAppException
{
    public override int StatusCode => 409;
    
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : NotesAppException
{
    public override int StatusCode => 401;
    
    public UnauthorizedException(string message = "Unauthorized access.") : base(message) { }
}

public class ForbiddenException : NotesAppException
{
    public override int StatusCode => 403;
    
    public ForbiddenException(string message = "Access forbidden.") : base(message) { }
}