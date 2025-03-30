using Microsoft.Extensions.Options;

namespace Pocket.Infrastructure.CookiePasswordHistory;

public class PasswordHistoryHandler(IOptions<PasswordHistoryOptions> options)
{
    private const string CookieKey = "pg";

    private static readonly CookieOptions CookieOptions = new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(1)
    };

    public string? Find(HttpContext httpContext, string noteId)
    {
        return Parse(httpContext)?.NotePasswords.GetValueOrDefault(noteId);
    }

    public void Append(HttpContext httpContext, string noteId, string password)
    {
        var passwordHistory = Parse(httpContext) ?? new PasswordHistory(new Dictionary<string, string>());
        passwordHistory.NotePasswords[noteId] = password;
        var value = options.Value.PasswordHistoryFormat.Protect(passwordHistory);
        httpContext.Response.Cookies.Append(CookieKey, value, CookieOptions);
    }

    private PasswordHistory? Parse(HttpContext httpContext)
    {
        var cookie = httpContext.Request.Cookies[CookieKey];
        return options.Value.PasswordHistoryFormat.Unprotect(cookie);
    }
}