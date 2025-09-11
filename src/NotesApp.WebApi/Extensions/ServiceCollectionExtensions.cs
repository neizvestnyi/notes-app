using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using NotesApp.Data;
using NotesApp.Data.Interfaces;
using NotesApp.Data.Repositories;
using NotesApp.Domain.Interfaces;
using NotesApp.Domain.Services;
using NotesApp.WebApi.Authentication;

namespace NotesApp.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var useDevAuth = configuration.GetValue<bool>("UseDevAuthentication", false);
        
        if (useDevAuth && isDevelopment)
        {
            // Development mode - minimal auth for testing
            services.AddAuthentication("Development")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>(
                    "Development", options => { });
        }
        else
        {
            // Production mode - Azure AD authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));
        }

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSPA", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Notes API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new()
            {
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Please enter JWT token",
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INoteRepository, NoteRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();
        return services;
    }
}