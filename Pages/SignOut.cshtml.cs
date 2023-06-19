using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRatingSystemWebApp.Models;
using System.Security.Claims;

namespace StudentRatingSystemWebApp.Pages
{
    public class SignOutModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/SignIn");
        }
    }
}
