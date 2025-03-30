using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pocket.Application;
using Pocket.Infrastructure.CookiePasswordHistory;

namespace Pocket.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    [Required, StringLength(2000)]
    public string NoteContent { get; set; } = null!;

    [BindProperty]
    [Required, StringLength(200)]
    public string Password { get; set; } = null!;

    public async Task<IActionResult> OnPost(
        [FromServices] SecureNoteService secureNoteService,
        [FromServices] PasswordHistoryHandler passwordHistoryHandler
    )
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var noteId = await secureNoteService.CreateSecureNote(NoteContent, Password);

        var noteUri = new UriBuilder
        {
            Scheme = Request.Scheme,
            Host = Request.Host.Host,
            Port = Request.Host.Port ?? -1,
            Path = $"s/{noteId}",
        };

        passwordHistoryHandler.Append(HttpContext, noteId, Password);

        return Redirect(noteUri.ToString());
    }
}