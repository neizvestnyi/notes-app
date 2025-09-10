using System.Net;
using System.Text.Json;
using NotesApp.Api.Exceptions;
using NotesApp.Api.Models;

namespace NotesApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        
        _logger.LogError(exception, 
            "An error occurred while processing request {TraceId}: {Message}", 
            traceId, exception.Message);

        var response = context.Response;
        
        // Don't try to write to response if it has already started
        if (response.HasStarted)
        {
            return;
        }

        response.Clear();
        response.ContentType = "application/json";

        var apiResponse = exception switch
        {
            NotesAppException customEx => new ApiResponse
            {
                Success = false,
                Message = customEx.Message,
                Errors = customEx is ValidationException validationEx ? validationEx.Errors : null,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId
            },
            _ => new ApiResponse
            {
                Success = false,
                Message = "An internal server error occurred.",
                Timestamp = DateTime.UtcNow,
                TraceId = traceId
            }
        };

        response.StatusCode = exception switch
        {
            NotesAppException customEx => customEx.StatusCode,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}