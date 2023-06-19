using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;
using System.Text.Json;

namespace StudentRatingSystemWebApp.Pages
{
    public class ScholarshipsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public int AcademicScholarshipsRate { get => int.Parse(_configuration["AcademicScholarshipsRate"]); }

        public List<ScholarshipType> scholarshipTypes;
        public List<Student> students;
        public ScholarshipsModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _configuration = configuration;
            _environment = env;
        }
        public IActionResult OnGet()
        {
			if (!CurrentUserCanEditScholarships())
			{
                return Forbid();
			}
            using(var db = new StudentRatingDbContext())
			{
                scholarshipTypes = db.ScholarshipTypes.ToList();
                students = db.Students
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .ToList()
                    .OrderBy(s => s.Name)
                    .OrderByDescending(s => s.GroupId)
                    .ToList();
			}
            return null;
        }

        public IActionResult OnGetAuto()
        {
            if (!CurrentUserCanEditScholarships())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                scholarshipTypes = db.ScholarshipTypes.ToList();
                students = db.Students
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .Include(s => s.Marks).ThenInclude(m => m.Cource)
                    .Include(s => s.AdditionalPoints)
                    .ToList()
                    .OrderBy(s => s.Name)
                    .OrderByDescending(s => s.GroupId)
                    .ToList();
                ScholarshipCalculator scholarshipCalculator = PageContext.HttpContext.RequestServices.GetService<ScholarshipCalculator>();
                scholarshipCalculator.AcademicScholarshipsRate = int.Parse(_configuration["AcademicScholarshipsRate"]);
                foreach(var student in students)
                {
                    if (scholarshipCalculator.GetListOfAcademicScholarships(students).Contains(student))
                    {
                        if (student.IsExcellentStudent())
                        {
                            student.ScholarshipTypeId = 2;
                        }
                        else
                        {
                            student.ScholarshipTypeId = 1;
                        }
                    }
                    else
                    {
                        if(student.ScholarshipTypeId != 3)
                        {
                            student.ScholarshipTypeId = 0;
                        }
                    }
                }
                db.Students.UpdateRange(students);
                db.SaveChanges();
            }
            return null;
        }

        public IActionResult OnPostSize(int id, int size)
		{
            if (!CurrentUserCanEditScholarships())
            {
                return Forbid();
            }
            using(var db = new StudentRatingDbContext())
			{
                ScholarshipType type = db.ScholarshipTypes.Find(id);
                if(type == null)
				{
                    return NotFound();
				}
                if(size < 0)
				{
                    return NotFound();
				}
                type.Size = size;
                db.ScholarshipTypes.Update(type);
                db.SaveChanges();
                scholarshipTypes = db.ScholarshipTypes.ToList();
                students = db.Students
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .ToList()
                    .OrderBy(s => s.Name)
                    .OrderByDescending(s => s.GroupId)
                    .ToList(); ;
            }
            return null;
        }

        public IActionResult OnPostPercent(int percent)
		{
            if (!CurrentUserCanEditScholarships())
            {
                return Forbid();
            }
            if(percent < 0 || percent > 100)
			{
                return null;
			}
            SetAcademicScholarshipsRate(percent);
            Thread.Sleep(1000);
            return RedirectToPage("/Scholarships");
        }

        private void SetAcademicScholarshipsRate(int value)
        {
            string path = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            string json = System.IO.File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Dictionary<string,object>>(json);
            config["AcademicScholarshipsRate"] = value.ToString();
            json = JsonSerializer.Serialize(config);
            System.IO.File.WriteAllText(path, json);
        }

        public IActionResult OnPostStudent(int id, int scholarshiptype)
        {
            if (!CurrentUserCanEditScholarships())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Student student = db.Students.Find(id);
                if(student == null)
                {
                    return NotFound();
                }
                student.ScholarshipTypeId = scholarshiptype;
                db.Students.Update(student);
                db.SaveChanges();
                scholarshipTypes = db.ScholarshipTypes.ToList();
                students = db.Students
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .ToList()
                    .OrderBy(s => s.Name)
                    .OrderByDescending(s => s.GroupId)
                    .ToList();
            }
            return null;
        }

        public List<SelectListItem> GetScholarshipOptions(int currentValue)
		{
            return scholarshipTypes.Select(sct => new SelectListItem()
            {
                Value = sct.Id.ToString(),
                Text = sct.Name,
                Selected = sct.Id == currentValue
            }).ToList();
		}

        public bool CurrentUserCanEditScholarships()
        {
            return GetCurrent().Role == Role.ADMIN || GetCurrent().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_SCHOLARSHIP);
        }
        private User GetCurrent()
        {
            using (var db = new StudentRatingDbContext())
            {
                return db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
            }
        }
    }
}
