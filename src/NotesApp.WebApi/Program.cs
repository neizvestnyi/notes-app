using NotesApp.Data;
using NotesApp.WebApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddValidation();
builder.Services.AddAuthenticationServices(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddCustomCors();
builder.Services.AddApiDocumentation();

// Add HTTP context accessor and memory caching
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotesDbContext>("database");

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSPA");
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Detailed health check endpoint
var useDevAuth = builder.Configuration.GetValue<bool>("UseDevAuthentication", false);
app.MapGet("/health/detailed", () => 
{
    var healthData = new { 
        status = "healthy", 
        timestamp = DateTime.UtcNow, 
        environment = app.Environment.EnvironmentName,
        authMode = useDevAuth ? "development" : "production",
        version = "1.0.0"
    };
    return Results.Ok(healthData);
})
.WithName("DetailedHealthCheck")
.WithOpenApi()
.WithSummary("Detailed health check")
.WithDescription("Returns detailed health status including version and environment info");

// Auth info endpoint for debugging (development only)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/auth-info", (HttpContext context) => 
    {
        var authInfo = new { 
            authenticationMode = useDevAuth ? "development" : "azuread",
            isAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
            userName = context.User.Identity?.Name,
            claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
        return Results.Ok(authInfo);
    })
    .WithName("AuthInfo")
    .WithOpenApi()
    .WithSummary("Authentication debug info")
    .WithDescription("Development-only endpoint for debugging authentication");
}

try
{
    Log.Information("Starting Notes API (Clean Architecture)");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
