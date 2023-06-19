using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRatingSystemWebApp.Models;
using System.Security.Claims;

namespace StudentRatingSystemWebApp.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly ILogger<SignUpModel> _logger;

        public SignUpModel(ILogger<SignUpModel> logger)
        {
            _logger = logger;
        }
        public bool showWarning;
        public void OnGet(bool showWarning=false)
        {
            this.showWarning = showWarning;
        }
        public async Task<IActionResult> OnPostAsync(string login, string password)
        {
            if (login == null || password == null)
            {
                return BadRequest("Не всі поля заповнені");
            }
            using (var db = new StudentRatingDbContext())
            {
                HashCalculator hashCalculator = PageContext.HttpContext.RequestServices.GetService<HashCalculator>();
                User user = db.Users.Where(u => u.Login == login && u.Password == hashCalculator.CalculateHash(password)).FirstOrDefault();
                if(user == null)
                {
                    return RedirectToPage(new{ showWarning = true });
                }
                else
                {
                    var claims = new List<Claim> { new Claim("ID", user.Id.ToString()), new Claim(ClaimTypes.Name, login), new Claim(ClaimTypes.Role, user.Role.ToString()) };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    await PageContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                }
            }
            return RedirectToPage("/Index");
        }
    }
}
