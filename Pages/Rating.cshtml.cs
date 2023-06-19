using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StudentRatingSystemWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentRatingSystemWebApp.Pages
{
    public class RatingModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public RatingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<Student> Students { get; set; }

        public IActionResult OnGet(int specialty=0, int cource=0)
        {
            using ( var db = new StudentRatingDbContext())
            {
                User user = db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
                if (user.Role == Role.STUDENT)
                {
                    specialty = db.Students
                        .Include(s=>s.Group).ThenInclude(g => g.Specialty)
                        .ToList().First(x => x.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value)).Group.SpecialtyId;
                    cource = db.Students
                        .Include(s => s.Group).ToList().First(x => x.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value))
                        .Group.GetCurrentCource();
                }else if (PageContext.HttpContext.User.IsInRole("TEACHER") && specialty==0 && cource == 0)
                {
                    Group? curatedGroup = db.Teachers
                        .Include(t => t.CurratedGroup)
                        .First(t => t.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value)).CurratedGroup;
                    if( curatedGroup is not null)
                    {
                        specialty = curatedGroup.SpecialtyId;
                        cource = curatedGroup.GetCurrentCource();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else if (! (user.Permissions.Any(p => p.Id == (int)Permissions.VIEW_ALL_POINTS) || user.Role == Role.ADMIN))
                {
                    return Forbid();
                }
                Students = db.Students
                    .Where(x => x.Group.Specialty.Id == specialty)
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .Include(s => s.AdditionalPoints)
                    .Include(s => s.Scholarship)
                    .Include(s => s.Marks).ThenInclude(m => m.Cource).ToList();
                Students = Students.Where(x => x.Group.GetCurrentCource() == cource).ToList();
                return null;
            }
        }

        public string GetTableLineColor(Student student)
        {
            if (student.CountOfAcademicArrears() > 0)
            {
                return "table-danger  border-secondary";
            }
            if (student.IsExcellentStudent())
            {
                return "table-success  border-secondary";
            }
            ScholarshipCalculator scholarshipCalculator = PageContext.HttpContext.RequestServices.GetService<ScholarshipCalculator>();
            scholarshipCalculator.AcademicScholarshipsRate = int.Parse(_configuration["AcademicScholarshipsRate"]);
            if (scholarshipCalculator.GetListOfAcademicScholarships(Students).Contains(student))
            {
                return "table-primary  border-secondary";
            }
            if(student.Scholarship.Name == "—Óˆ≥‡Î¸Ì‡")
            {
                return "table-warning  border-secondary";
            }
            if (!student.IsOnBudget)
            {
                return "table-info  border-secondary";
            }
            return "";
        }

        public string GetTableLineColor(Student student, int semester)
        {
            if (student.CountOfAcademicArrears(semester) > 0)
            {
                return "table-danger  border-secondary";
            }
            if (student.WasExcellentStudent(semester))
            {
                return "table-success  border-secondary";
            }
            ScholarshipCalculator scholarshipCalculator = PageContext.HttpContext.RequestServices.GetService<ScholarshipCalculator>();
            scholarshipCalculator.AcademicScholarshipsRate = int.Parse(_configuration["AcademicScholarshipsRate"]);
            if (scholarshipCalculator.GetListOfAcademicScholarships(Students, semester).Contains(student))
            {
                return "table-primary  border-secondary";
            }
            if (student.Scholarship.Name == "—Óˆ≥‡Î¸Ì‡")
            {
                return "table-warning  border-secondary";
            }
            if (!student.IsOnBudget)
            {
                return "table-info  border-secondary";
            }
            return "";
        }
    }
}
