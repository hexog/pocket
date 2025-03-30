using Microsoft.AspNetCore.Authentication;

namespace Pocket.Infrastructure.CookiePasswordHistory;

public class PasswordHistoryOptions
{
    public ISecureDataFormat<PasswordHistory> PasswordHistoryFormat { get; set; } = null!;
}