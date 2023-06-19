using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class DepartmentsModel : PageModel
    {
        public Department editedDepartment;
        public List<Department> departments;
        public void OnGet(int facultyId = 0)
        {
            using (var db = new StudentRatingDbContext())
            {
                departments = db.Departments.Include(d => d.Teachers).Include(d => d.Faculty).ToList();
            }
        }

        public IActionResult OnGetDepartment(int id = 0)
        {
            if (!CurrentUserCanEditDepartments())
            {
                return Forbid();
            }
            if (id == 0)
            {
                editedDepartment = new Department() { Name = "", ShortName = "", FacultyId = 0, HeadOfDepartmentId = 0, Id = 0 };
            }
            else
            {
                using (var db = new StudentRatingDbContext())
                {
                    editedDepartment = db.Departments.Find(id);
                    if (editedDepartment == null)
                    {
                        return NotFound();
                    }
                }
            }
            return null;

        }

        public IActionResult OnPostDepartment(int id, string name, string shortname, int faculty, int head)
		{
            if (!CurrentUserCanEditDepartments())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
			{
                Department dep = new Department() { Id = id, Name = name, ShortName = shortname??"", FacultyId = faculty, HeadOfDepartmentId = head };
                if(id == 0)
				{
                    db.Departments.Add(dep);
				}
				else
                {
                    db.Departments.Update(dep);
                }
                db.SaveChanges();
                departments = db.Departments.Include(d => d.Teachers).Include(d => d.Faculty).ToList();
			}
            return null;
		}

        public IActionResult OnPostDelete(int id)
		{
            if (!CurrentUserCanEditDepartments())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
			{
                Department dep = db.Departments.Find(id);
                db.Departments.Remove(dep);
                db.SaveChanges();
                departments = db.Departments.Include(d => d.Teachers).Include(d => d.Faculty).ToList();
            }
            return null;
		}

        public List<SelectListItem> GetFacultyOptions()
		{
            using (var db = new StudentRatingDbContext())
            {
                return db.Faculties.OrderBy(f => f.Name).Select(f => new SelectListItem()
                {
                    Value = f.Id.ToString(),
                    Text = f.Name.ToString(),
                    Selected = f.Id == editedDepartment.FacultyId
                }).ToList();
            }
        }

        public List<SelectListItem> GetHeadOfDepartmentOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                return db.Teachers.Where(t => t.DepartmentId == editedDepartment.Id).OrderBy(t => t.Name).Select(t => new SelectListItem()
                {
                    Value = t.Id.ToString(),
                    Text = t.Name.ToString(),
                    Selected = t.Id == editedDepartment.HeadOfDepartmentId
                }).ToList();
            }
        }

        public bool CurrentUserCanEditDepartments()
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
