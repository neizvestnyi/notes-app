using System.Diagnostics;
using System.Text;

namespace NotesApp.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequestAsync(context);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            await LogResponseAsync(context, stopwatch.ElapsedMilliseconds);
            
            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        var request = context.Request;
        
        var logData = new
        {
            TraceId = context.TraceIdentifier,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            UserAgent = request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.Identity?.Name,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength
        };

        _logger.LogInformation("HTTP Request: {@RequestData}", logData);

        // Log request body for POST/PUT requests (but not for large payloads)
        if ((request.Method == "POST" || request.Method == "PUT") && 
            request.ContentLength.HasValue && request.ContentLength.Value < 10000) // Don't log large payloads
        {
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength.Value)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;

            if (!string.IsNullOrEmpty(requestBody))
            {
                _logger.LogDebug("Request Body: {RequestBody}", requestBody);
            }
        }
    }

    private async Task LogResponseAsync(HttpContext context, long elapsedMs)
    {
        var response = context.Response;
        
        response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        var logData = new
        {
            TraceId = context.TraceIdentifier,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            ElapsedMilliseconds = elapsedMs,
            UserId = context.User?.Identity?.Name
        };

        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, "HTTP Response: {@ResponseData}", logData);

        // Log response body for errors (but not for large responses)
        if (response.StatusCode >= 400 && responseBody.Length < 5000)
        {
            _logger.LogDebug("Response Body: {ResponseBody}", responseBody);
        }
    }
}