using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
	public class FacultyModel : PageModel
    {
        public Faculty editedFaculty;
        public List<Faculty> faculties;
        public void OnGet()
        {
            using (var db = new StudentRatingDbContext())
            {
                faculties = db.Faculties.Include(f => f.Departments).ThenInclude(d => d.Teachers).ToList();
            }
        }

        public IActionResult OnGetFaculty(int id = 0)
        {
            if (!CurrentUserCanEditFaculties())
            {
                return Forbid();
            }
            if (id == 0)
            {
                editedFaculty = new Faculty() { Name = "", ShortName = "", DeanId = 0 };
            }
            else
            {
                using (var db = new StudentRatingDbContext())
                {
                    editedFaculty = db.Faculties.Find(id);
                    if (editedFaculty == null)
                    {
                        return NotFound();
                    }
                }
            }
            return null;

        }

        public IActionResult OnPostFaculty(int id, string name, string shortname, int dean)
        {
            if (!CurrentUserCanEditFaculties())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Faculty faculty = new Faculty() { Id = id, Name = name, ShortName = shortname, DeanId = dean };
                if (id == 0)
                {
                    db.Faculties.Add(faculty);
                }
                else
                {
                    db.Faculties.Update(faculty);
                }
                db.SaveChanges();
                faculties = db.Faculties.Include(f => f.Departments).ThenInclude(d => d.Teachers).ToList();
            }
            return null;
        }

        public IActionResult OnPostDelete(int id)
        {
            if (!CurrentUserCanEditFaculties())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Faculty faculty = db.Faculties.Find(id);
                db.Faculties.Remove(faculty);
                db.SaveChanges();
                faculties = db.Faculties.Include(f => f.Departments).ThenInclude(d => d.Teachers).ToList();
            }
            return null;
        }

        public List<SelectListItem> GetDeanOfFacultyOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                return db.Teachers.Include(t => t.Department).Where(t => t.Department.FacultyId == editedFaculty.Id)
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem()
                {
                    Value = t.Id.ToString(),
                    Text = t.Name.ToString(),
                    Selected = t.Id == editedFaculty.DeanId
                }).ToList();
            }
        }

        public bool CurrentUserCanEditFaculties()
        {
            return GetCurrentUser().Role == Role.ADMIN || GetCurrentUser().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_GROUPS);
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
