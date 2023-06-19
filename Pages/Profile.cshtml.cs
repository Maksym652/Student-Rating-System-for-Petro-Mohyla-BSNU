using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class ProfileModel : PageModel
    {
        public User user;
        public bool isCurrent;
        public IActionResult OnGet(int id = 0)
        {
            isCurrent = id == 0;
            if (isCurrent)
            {
                id = int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value);
            }
            user = FindUser(id);
            if(user == null)
            {
                return NotFound();
            }
            return null;
        }

        public IActionResult OnPost(int id, string contacts)
        {
            if(int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value) != id){
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                User user = db.Users.Find(id);
                user.Contacts = contacts;
                db.Users.Update(user);
                db.SaveChanges();
            }
            user = FindUser(id);
            isCurrent = true;
            return null;
        }

        public IActionResult OnPostPermissions(int id, int[] permissions)
        {
            if (!PageContext.HttpContext.User.IsInRole("ADMIN"))
            {
                return Forbid();
            }
            using( var db = new StudentRatingDbContext())
            {
                User user = db.Users.Include(u => u.Permissions).First(u => u.Id == id);
                user.Permissions = new List<Permission>();
                List<Permission> allPermissions = db.Permissions.ToList();
                foreach(var permissionId in permissions)
                {
                    user.Permissions.Add(allPermissions[permissionId-1]);
                }
                db.SaveChanges();
            }
            user = FindUser(id);
            return null;
        }

        private User FindUser(int id)
        {
            User user;
            using (var db = new StudentRatingDbContext())
            {
                user = db.Users.Find(id);
                if (user.Role == Role.STUDENT)
                {
                    user = db.Students
                        .Include(s => s.Group).ThenInclude(g => g.Specialty)
                        .First(s => s.Id == id);
                }
                else if (user.Role == Role.TEACHER)
                {
                    user = db.Teachers
                        .Include(u => u.Permissions)
                        .Include(t => t.Department)
                        .Include(t => t.CurratedGroup)
                        .First(t => t.Id == id);
                }
                else
                {
                    user = db.Employees
                        .Include(u => u.Permissions)
                        .First(e => e.Id == id);
                }
            }
            return user;
        }

        public List<Permission> GetAllPermissions()
        {
            using(var db = new StudentRatingDbContext())
            {
                return db.Permissions.ToList();
            }
        }
    }
}