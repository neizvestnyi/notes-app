using Microsoft.AspNetCore.Mvc;
using NotesApp.Api.Models;
using System.Text.Json;

namespace NotesApp.Api.Extensions;

public static class ResultExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// Configures the ResultExtensions with the required HttpContextAccessor dependency.
    /// This should be called during application startup.
    /// </summary>
    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public static IResult ToApiResponse<T>(this T data, string? message = null)
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        response.TraceId = GetTraceId();
        return Results.Ok(response);
    }

    public static IResult ToCreatedApiResponse<T>(this T data, string location, string? message = null)
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        response.TraceId = GetTraceId();
        return Results.Created(location, response);
    }

    public static IResult ToNoContentApiResponse(string? message = null)
    {
        var response = ApiResponse.SuccessResponse(message ?? "Operation completed successfully");
        response.TraceId = GetTraceId();
        return Results.Ok(response);
    }

    public static IResult ToBadRequestApiResponse(string message, List<string>? errors = null)
    {
        var response = ApiResponse.ErrorResponse(message, errors);
        response.TraceId = GetTraceId();
        return Results.BadRequest(response);
    }

    public static IResult ToNotFoundApiResponse(string message)
    {
        var response = ApiResponse.ErrorResponse(message);
        response.TraceId = GetTraceId();
        return Results.NotFound(response);
    }

    public static IResult ToValidationErrorResponse(List<string> errors)
    {
        var response = ApiResponse.ErrorResponse(errors);
        response.TraceId = GetTraceId();
        return Results.BadRequest(response);
    }

    public static IResult ToPaginatedApiResponse<T>(this PaginatedResponse<T> data, string? message = null)
    {
        var response = ApiResponse<PaginatedResponse<T>>.SuccessResponse(data, message);
        response.TraceId = GetTraceId();
        return Results.Ok(response);
    }

    private static string GetTraceId()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        return httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
    }
}