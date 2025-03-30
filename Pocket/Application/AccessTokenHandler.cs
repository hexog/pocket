using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Pocket.Application;

public partial class AccessTokenHandler(
    IOptions<PocketAuthenticationOptions> authenticationOptions,
    IOptions<AccessTokenOptions> accessTokenOptions,
    ILogger<AccessTokenHandler> logger
)
{
    public (string AccessToken, string AccessTokenHash) GenerateAccessToken(string principal, string authenticationType)
    {
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, principal),
                        new Claim(ClaimTypes.Sid, Guid.NewGuid().ToString())
                    ],
                    authenticationType
                )
            ),
            "AccessToken"
        );

        ticket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);

        var accessToken = accessTokenOptions.Value.TicketDataFormat.Protect(ticket);
        var hash = Hash(accessToken);
        return (accessToken, hash);
    }

    public AuthenticationTicket? AuthenticateAccessToken(string accessToken)
    {
        var accessTokenHash = Hash(accessToken);
        if (!authenticationOptions.Value.AllowedTokenHashSet.Contains(accessTokenHash))
        {
            LogAccessTokenNotAllowed(accessTokenHash);
            return null;
        }

        var ticket = accessTokenOptions.Value.TicketDataFormat.Unprotect(accessToken);
        var now = DateTimeOffset.UtcNow;
        if (ticket?.Properties.ExpiresUtc is not { } ticketExpiresUtc || ticketExpiresUtc < now)
        {
            LogAccessTokenNotAllowed(accessTokenHash);
            return null;
        }

        return ticket;
    }

    private static string Hash(string accessToken)
    {
        return Convert.ToHexStringLower(SHA512.HashData(Encoding.UTF8.GetBytes(accessToken)));
    }

    [LoggerMessage(LogLevel.Information, "Access token {AccessTokenHash} is not allowed")]
    private partial void LogAccessTokenNotAllowed(string accessTokenHash);

    [LoggerMessage(LogLevel.Information, "Invalid access token {AccessTokenHash}")]
    private partial void LogInvalidAccessToken(string accessTokenHash);
}