using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRatingSystemWebApp;
using StudentRatingSystemWebApp.Models;
using System.Text.RegularExpressions;

namespace StudentRatingSystemWebApp.Pages
{
    public class ChangePasswordModel : PageModel
    {

        public int userId;
        public string message;
        public static Regex passwordRegex = new Regex("^[A-Za-z0-9]$");
        public void OnGet(int id, string message="")
        {
            userId = id;
            this.message = message;
        }
        
        public IActionResult OnPost(int userId, string password, string newPassword, string repeatPassword)
		{
            if (newPassword != repeatPassword)
            {
                return RedirectToPage("/ChangePassword", new { id = userId, message = "Паролі не збігаються!" });
            }
            if (passwordRegex.IsMatch(password))
            {
                return RedirectToPage("/ChangePassword", new { id = userId, message = "Пароль повинен містити тільки латинські літери та цифри" });
            }
            using (var db = new StudentRatingDbContext())
			{

                HashCalculator hashCalculator = PageContext.HttpContext.RequestServices.GetService<HashCalculator>();
                User user = db.Users.Find(userId);
                if(hashCalculator.CalculateHash(password) != user.Password)
				{
                    return RedirectToPage("/ChangePassword", new {id = userId, message="Неправильний пароль!"} );
                }
                user.Password = hashCalculator.CalculateHash(password);
                db.Users.Update(user);
                db.SaveChanges();
			}
            return RedirectToPage("/Profile");
		}

    }
}
