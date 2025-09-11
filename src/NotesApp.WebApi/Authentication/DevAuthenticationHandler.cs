using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace NotesApp.WebApi.Authentication;

public class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user"),
            new Claim(ClaimTypes.Name, "Development User"),
            new Claim(ClaimTypes.Email, "dev@example.com")
        };

        var identity = new ClaimsIdentity(claims, "Development");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Development");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}