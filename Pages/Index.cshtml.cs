using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public List<string> documents;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public IndexModel(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _environment = env;
        }
        public void OnGet()
        {
            documents = new List<string>();
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "files\\documents")))
            {
                documents.Add(file.Replace(_environment.WebRootPath+"\\",""));
            }
            if(documents.Count > 10)
            {
                documents.RemoveRange(9, documents.Count - 9);
            }
        }

        public bool CurrentUserCanEditScholarships()
        {
            return GetCurrentUser().Role == Role.ADMIN || GetCurrentUser().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_SCHOLARSHIP);
        }
        private User GetCurrentUser()
        {
            using (var db = new StudentRatingDbContext())
            {
                return db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
            }
        }
    }
}