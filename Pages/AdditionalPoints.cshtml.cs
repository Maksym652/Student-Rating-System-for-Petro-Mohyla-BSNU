using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class AdditionalPointsModel : PageModel
    {
        public List<AdditionalPoint> additionalPoints;
        public AdditionalPoint? editedPoint;

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public AdditionalPointsModel(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _environment = env;
        }

        public void OnGet()
        {
            using(var db = new StudentRatingDbContext())
            {
                additionalPoints = db.AdditionalPoints.Include(p => p.Student).ThenInclude(s => s.Group).OrderByDescending(p => p.Date).ToList();
            }
        }

        public IActionResult OnGetEdit(int id)
        {
            if (!CurrentUserCanEditAdditionalPoints())
            {
                return Forbid();
            }
            if(id == 0)
            {
                editedPoint = new AdditionalPoint() { StudentId = 0, Date=DateTime.Now, Id=0, Description = "", Point = 0, Type = AdditionalPointType.PUBLIC_ACTIVITY};
            }
            else
            {
                using (var db = new StudentRatingDbContext())
                {
                    editedPoint = db.AdditionalPoints.Include(p => p.Student).First(p => p.Id == id);
                }
            }
            return null;
        }

        public IActionResult OnPostDelete(int id)
        {
            if (!CurrentUserCanEditAdditionalPoints())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                AdditionalPoint ap = db.AdditionalPoints.Find(id);
                if(ap == null)
                {
                    return NotFound();
                }
                if (ap.HasConfirmationDocument())
                {
                    string previousFileName = db.AdditionalPoints.Find(id).ConfirmationFileName;
                    string previousFilePath = Path.Combine(_environment.WebRootPath, "files/additionalPointsConfirmations", previousFileName);
                    System.IO.File.Delete(previousFilePath);
                }
                db.AdditionalPoints.Remove(ap);
                db.SaveChanges();
                additionalPoints = db.AdditionalPoints.Include(p => p.Student).ThenInclude(s => s.Group).OrderByDescending(p => p.Date).ToList();
            }
            return null;
        }

        public IActionResult OnPostEdit(int id, int type,string description, string date, string point, IFormFile document, int student)
        {
            if (!CurrentUserCanEditAdditionalPoints())
            {
                return Forbid();
            }
            AdditionalPoint ap = new AdditionalPoint() { Id = id, Type = (AdditionalPointType)type, Date = DateTime.Parse(date), StudentId = student, Description = description};
            float tmp = 0;
            if(!float.TryParse(point, out tmp))
            {
                float.TryParse(point.Replace('.', ','), out tmp);
            }
            ap.Point = tmp;
            using(var db = new StudentRatingDbContext())
            {
                ap.Student = db.Students.Find(student);
            }
            if (document != null)
            {
                ap.ConfirmationFileName = ap.GetDefaultConfirmationFileName()+Path.GetExtension(document.FileName);
                string path = Path.Combine(_environment.WebRootPath, "files/additionalPointsConfirmations", ap.ConfirmationFileName);
                using(var filestream = new FileStream(path, FileMode.Create))
                {
                    document.CopyTo(filestream);
                }
                if(id != 0)
                {
                    using (var db = new StudentRatingDbContext())
                    {
                        if (db.AdditionalPoints.Find(id).HasConfirmationDocument())
                        {
                            string previousFileName = db.AdditionalPoints.Find(id).ConfirmationFileName;
                            string previousFilePath = Path.Combine(_environment.WebRootPath, "files/additionalPointsConfirmations", previousFileName);
                            System.IO.File.Delete(previousFilePath);
                        }
                    }
                }
            }
            else
            {
                if(id != 0)
				{
                    using (var db = new StudentRatingDbContext())
                    {
                        ap.ConfirmationFileName = db.AdditionalPoints.Find(id).ConfirmationFileName ?? null;
                    }
                }
            }
            ap.Student = null;
            using(var db = new StudentRatingDbContext())
            {
                if(id != 0)
                {
                    db.AdditionalPoints.Update(ap);
                }
                else
                {
                    db.AdditionalPoints.Add(ap);
                }
                db.SaveChanges();
                additionalPoints = db.AdditionalPoints.Include(p => p.Student).ThenInclude(s => s.Group).OrderByDescending(p => p.Date).ToList();
            }
            return null;
        }

        public bool CurrentUserCanEditAdditionalPoints()
        {
            return GetCurrentUser().Role == Role.ADMIN || GetCurrentUser().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_ADDITIONAL_POINTS);
        }
        private User GetCurrentUser()
        {
            using (var db = new StudentRatingDbContext())
            {
                return db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
            }
        }

        public List<SelectListItem> GetStudentOptions()
        {
            using(var db = new StudentRatingDbContext())
            {
                List<SelectListGroup> groups = db.Groups.Select(g => new SelectListGroup(){ 
                    Name = g.GetCurrentGroupNumber().ToString()
                }).ToList();
                List<Student> students = db.Students.Include(s => s.Group).ToList();
                List<SelectListItem> items = students.Select(s => new SelectListItem()
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Group = groups.FindAll(x => x.Name == s.Group.GetCurrentGroupNumber().ToString()).First(),
                    Selected = s.Id == editedPoint.StudentId
                }).ToList();
                items.Sort((x, y) =>
                {
                    if (x.Group == y.Group)
                    {
                        return x.Text.CompareTo(y.Text);
                    }
                    return x.Group.Name.CompareTo(y.Group.Name);
                });
                return items;
            }
        }
    }
}
