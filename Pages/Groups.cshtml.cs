using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class GroupsModel : PageModel
    {

        public List<Group> groups;
        public Group? editedGroup;

        public List<SelectListItem> specialties;
        public List<SelectListItem> teachers;
        public List<SelectListItem> faculties;

        public void OnGet()
        {
            using(var db = new StudentRatingDbContext())
			{
                groups = db.Groups.Include(g => g.Specialty).Include(g => g.Courator).ToList().OrderBy(g => g.SpecialtyId).OrderBy(g => g.GetCurrentGroupNumber()).ToList();
			}
        }

        public IActionResult OnGetEdit(int id)
		{
            if (!CurrentUserCanEditGroups())
            {
                return Forbid();
            }
            if (id == 0)
            {
                editedGroup = new Group() { Id=0, CouratorId = 0, FacultyId = 0, EducationForm = EducationForm.FULL_TIME_DAY, SpecialtyId = 0};
			}
			else
			{
                using(var db = new StudentRatingDbContext())
				{
                    editedGroup = db.Groups.Find(id);
                    if(editedGroup is null)
                    {
                        return NotFound();
                    }
                }
			}
            using (var db = new StudentRatingDbContext())
            {
                specialties = db.Specialties.OrderBy(s => s.Id).Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Id + " " + s.Name,
                    Selected = s.Id == editedGroup.SpecialtyId
                }).ToList();
                teachers = GetTeacherOptions();
                faculties = db.Faculties.OrderBy(f => f.Name).Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name.ToString(),
                    Selected = f.Id == editedGroup.FacultyId
                }).ToList();
            }
            return null;
        }

        private List<SelectListItem> GetTeacherOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                Dictionary<int, SelectListGroup> departments = new Dictionary<int, SelectListGroup>();
                foreach (var item in db.Departments.OrderBy(d => d.Name).ToList())
                {
                    departments.Add(item.Id, new SelectListGroup() { Name = item.ShortName == "" ? item.Name : "кафедра " + item.ShortName });
                }
                return db.Teachers.OrderBy(t => t.Name).Select(t => new SelectListItem()
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == editedGroup.CouratorId,
                    Group = departments[t.DepartmentId]
                }).ToList();
            }
        }

        public IActionResult OnPostGroup(int id, int studyForm, int specialty, int courator, int faculty)
		{
            if (!CurrentUserCanEditGroups())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
			{
                Group group = db.Groups.Find(id);
                if(group == null)
				{
                    group = new Group() { Id = id };
                    group.EducationForm = (EducationForm)studyForm;
                    group.SpecialtyId = specialty;
                    group.CouratorId = courator;
                    group.FacultyId = faculty;
                    db.Groups.Add(group);
                }
				else
				{
                    group.EducationForm = (EducationForm)studyForm;
                    group.SpecialtyId = specialty;
                    if(courator != 0)
                    {
                        group.CouratorId = courator;
                    }
                    group.FacultyId = faculty;
                    db.Groups.Update(group);
                }
                db.SaveChanges();
                groups = db.Groups.Include(g => g.Specialty).Include(g => g.Courator).ToList();
            }
            return null;
		}

        public IActionResult OnPostDelete(int id)
        {
            if (!CurrentUserCanEditGroups())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Group group = db.Groups.Find(id);
                db.Groups.Remove(group);
                db.SaveChanges();
                groups = db.Groups.Include(g => g.Specialty).Include(g => g.Courator).ToList();
            }
            return null;
        }

        public bool CurrentUserCanEditGroups()
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
