using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NotesApp.Api.Authentication;

public class DevAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = "dev-user";
    public string DefaultUserName { get; set; } = "Development User";
    public string DefaultUserEmail { get; set; } = "dev@example.com";
}

public class DevAuthenticationHandler : AuthenticationHandler<DevAuthenticationSchemeOptions>
{
    public DevAuthenticationHandler(IOptionsMonitor<DevAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for dev auth header or accept any request in dev mode
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        
        // Accept either no auth header (for ease of testing) or "Bearer dev-token"
        if (authHeader == null || authHeader == "Bearer dev-token" || authHeader.StartsWith("Bearer dev-"))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Options.DefaultUserId),
                new Claim(ClaimTypes.Name, Options.DefaultUserName),
                new Claim(ClaimTypes.Email, Options.DefaultUserEmail),
                new Claim("scope", "Notes.ReadWrite")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid dev token"));
    }
}