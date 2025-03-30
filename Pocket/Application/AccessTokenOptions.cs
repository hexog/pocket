using Microsoft.AspNetCore.Authentication;

namespace Pocket.Application;

public class AccessTokenOptions
{
    public ISecureDataFormat<AuthenticationTicket> TicketDataFormat { get; set; } = null!;
}