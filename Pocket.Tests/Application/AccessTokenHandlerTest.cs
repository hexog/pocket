using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pocket.Application;

public class AccessTokenHandlerTest : TestBase
{
    [Test]
    public void TestGenerateValidAccessToken()
    {
        var accessTokenHandler = Services.GetRequiredService<AccessTokenHandler>();

        var (accessToken, accessTokenHash) = accessTokenHandler.GenerateAccessToken("test-principal", "test");

        Assert.Multiple(() =>
        {
            Assert.That(accessTokenHash, Has.Length.EqualTo(128).And.Matches("^[a-f0-9]{128}$"));
            Assert.That(accessToken, Does.Not.Contain("test-principal"));
        });

        var invalidTicket = accessTokenHandler.AuthenticateAccessToken(accessToken);
        Assert.That(invalidTicket, Is.Null);

        var options = Services.GetRequiredService<IOptions<PocketAuthenticationOptions>>().Value;
        options.AllowedTokenHashes = [accessTokenHash];
        options.AllowedTokenHashSet = options.AllowedTokenHashes.ToFrozenSet(StringComparer.Ordinal);

        var ticket = accessTokenHandler.AuthenticateAccessToken(accessToken);
        Assert.That(ticket, Is.Not.Null);
        Assert.That(ticket.Principal.FindFirstValue(ClaimTypes.Name), Is.EqualTo("test-principal"));
    }
}