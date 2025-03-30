using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pocket.Application;

namespace Pocket.Pages;

public class SignIn : PageModel
{
    [BindProperty]
    [Required]
    [StringLength(600, MinimumLength = 10)]
    public string? Token { get; set; }

    public IActionResult OnPost([FromServices] AccessTokenHandler accessTokenHandler)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Debug.Assert(Token is not null);
        var ticket = accessTokenHandler.AuthenticateAccessToken(Token);
        if (ticket is null)
        {
            ModelState.AddModelError(nameof(Token), "Invalid token");
            return Page();
        }

        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };

        return SignIn(ticket.Principal, authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}