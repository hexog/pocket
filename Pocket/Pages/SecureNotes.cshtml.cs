using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pocket.Application;
using Pocket.Infrastructure.CookiePasswordHistory;

namespace Pocket.Pages;

public class SecureNotesModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    [Required]
    public string NoteId { get; set; } = null!;

    [BindProperty]
    [Required]
    public string Password { get; set; } = null!;

    public string? NoteContent { get; set; }

    public async Task<IActionResult> OnGet(
        [FromServices] SecureNoteService secureNoteService,
        [FromServices] PasswordHistoryHandler passwordHistoryHandler
    )
    {
        if (!ModelState.IsValid)
        {
            return NotFound();
        }

        if (!await secureNoteService.AnySecureNote(NoteId))
        {
            return NotFound();
        }

        var passwordFromHistory = passwordHistoryHandler.Find(HttpContext, NoteId);
        if (passwordFromHistory is not null)
        {
            NoteContent = await secureNoteService.OpenSecureNote(NoteId, passwordFromHistory);
        }

        return Page();
    }

    public async Task<IActionResult> OnPost(
        [FromServices] SecureNoteService secureNoteService,
        [FromServices] PasswordHistoryHandler passwordHistoryHandler
    )
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var canOpenSecureNote = await secureNoteService.CanOpenSecureNote(NoteId, Password);
        if (!canOpenSecureNote)
        {
            ModelState.AddModelError(nameof(Password), "Invalid password.");
            return Page();
        }

        passwordHistoryHandler.Append(HttpContext, NoteId, Password);
        return RedirectToPage();
    }
}