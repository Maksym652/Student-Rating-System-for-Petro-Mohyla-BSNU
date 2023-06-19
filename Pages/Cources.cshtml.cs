using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class CourcesModel : PageModel
    {
        public List<Cource> userCources = new List<Cource>();
        public List<Cource> allCources = new List<Cource>();
        public Cource cource;
        public bool editMarksOnly = false;
        public IActionResult OnGet(int? id)
        {
            if (id is null || id == 0)
            {
                if (PageContext.HttpContext.User.IsInRole("STUDENT"))
                {
                    using (var db = new StudentRatingDbContext())
                    {
                        Student student = db.Students.Find(int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
                        userCources = db.Cources
                            .Where(c => c.GroupId == student.GroupId)
                            .Include(c => c.Marks)
                            .Include(c => c.Teacher)
                            .Include(c => c.Examiner)
                            .Include(c => c.Group).ThenInclude(g => g.Students).ThenInclude(s => s.Marks).ToList();
                    }
                }
                else if (PageContext.HttpContext.User.IsInRole("TEACHER"))
                {
                    using (var db = new StudentRatingDbContext())
                    {
                        userCources = db.Cources
                            .Include(c => c.Teacher)
                            .Include(c => c.Group).ThenInclude(g => g.Students)
                            .Include(c => c.Marks)
                            .Where(c => c.ExaminerId == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value)).ToList();
                        Teacher teacher = db.Teachers
                            .Include(t => t.Department).ThenInclude(d => d.Faculty)
                            .First(t => t.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
                        if (teacher.IsHeadOfDepartment())
                        {
                            allCources = db.Cources
                            .Include(c => c.Group).ThenInclude(g => g.Students).ThenInclude(s => s.Marks)
                            .Include(c => c.Teacher)
                            .Include(c => c.Examiner)
                            .Where(c => c.Teacher.DepartmentId == teacher.DepartmentId).ToList();
                        }
                        if (teacher.IsDeanOfFaculty())
                        {
                            allCources = db.Cources
                            .Include(c => c.Group).ThenInclude(g => g.Students).ThenInclude(s => s.Marks)
                            .Include(c => c.Teacher).ThenInclude(t => t.Department)
                            .Include(c => c.Examiner)
                            .Where(c => c.Teacher.Department.FacultyId == teacher.Department.FacultyId).ToList();
                        }
                    }
                }
                else
                {
                    using (var db = new StudentRatingDbContext())
                    {
                        allCources = db.Cources
                            .Include(c => c.Group).ThenInclude(g => g.Students).ThenInclude(s => s.Marks)
                            .Include(c => c.Teacher)
                            .Include(c => c.Examiner).ToList();
                    }
                }
                userCources.Sort((x, y) => { if (x.Year == y.Year) { return y.Semester - x.Semester; } return y.Year - x.Year; });
                allCources.Sort((x, y) => { if (x.Year == y.Year) { return y.Semester - x.Semester; } return y.Year - x.Year; });
            }
            else
            {
                if (PageContext.HttpContext.User.IsInRole("TEACHER"))
                {
                    editMarksOnly = true;
                    using (var db = new StudentRatingDbContext())
                    {
                        cource = db.Cources
                            .Include(c => c.Group).ThenInclude(g => g.Students).ThenInclude(s => s.Marks)
                            .First(c => c.Id == id);
                        if (cource is null)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            return null;
        }

        public IActionResult OnGetCource(int id)
        {
            if (id == 0)
            {
                cource = new Cource() { Name = "", ShortName = "", ExaminerId = 0, TeacherId = 0, FinalExamType = ExamType.CREDIT, GroupId = 0, Semester = AcademicSemester.Now.SemesterNumber, Year = DateTime.Now.Year, Id = 0 };
            }
            else
            {

                using (var db = new StudentRatingDbContext())
                {
                    cource = db.Cources
                        .Include(c => c.Group)
                        .Include(c => c.Teacher)
                        .Include(c => c.Examiner)
                        .First(c => c.Id == id);
                    if (cource is null)
                    {
                        return NotFound();
                    }
                }
            }
            return null;
        }


        public IActionResult OnGetInfo(int id)
        {
            using (var db = new StudentRatingDbContext())
            {
                cource = db.Cources
                    .Include(c => c.Group)
                    .Include(c => c.Teacher)
                    .Include(c => c.Examiner)
                    .First(c => c.Id == id);
                if (cource is null)
                {
                    return NotFound();
                }
            }
            return null;
        }

        public IActionResult OnPostPoint(int point, int studentId, string finalExamTime)
        {
            if (GetCurrentUser().Role != Role.TEACHER)
            {
                return Forbid();
            }
            int courceId = int.Parse(RouteData.Values["id"].ToString());
            using (var db = new StudentRatingDbContext())
            {
                Cource cource = db.Cources.Find(courceId);
                if(cource.ExaminerId != GetCurrentUser().Id)
                {
                    return Forbid();
                }
                Mark mark = db.Marks.Where(m => m.CourceId == courceId && m.StudentId == studentId).FirstOrDefault();
                if(mark == null)
                {
                    db.Marks.Add(new Mark()
                    {
                        StudentId = studentId,
                        CourceId = courceId,
                        ExamDate = DateTime.Parse(finalExamTime),
                        IsRetaken = false,
                        Point = 0
                    });
                    db.SaveChanges();
                }
                mark = db.Marks.Where(m => m.CourceId == courceId && m.StudentId == studentId).FirstOrDefault();
                mark.ExamDate = DateTime.Parse(finalExamTime);
                mark.IsRetaken = mark.ExamDate > cource.FinalExamTime;
                mark.Point = point;
                db.Marks.Update(mark);
                db.SaveChanges();
            }
            return RedirectToPage("/Cources", new { id = courceId });
        }

        public IActionResult OnPostCource(int id, string name, string shortName, int finalExamType, string finalExamTime, int semester, int year, string credits, int group, int teacher, int examiner, string practiceBegin, string practiceEnd)
        {
            if (!CurrentUserCanEditCources())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                if (finalExamType == (int)ExamType.PRACTICE)
                {
                    Practice practice = db.Practices.Find(id) ?? new Practice();
                    practice.Name = name;
                    practice.ShortName = shortName;
                    practice.FinalExamType = (ExamType)finalExamType;
                    practice.FinalExamTime = DateTime.Parse(finalExamTime);
                    practice.Semester = semester;
                    practice.Year = year;
                    float tmp = 0;
                    if (!float.TryParse(credits, out tmp))
                    {
                        float.TryParse(credits.Replace('.', ','), out tmp);
                    }
                    practice.Credits = tmp;
                    practice.GroupId = group;
                    practice.TeacherId = teacher;
                    practice.ExaminerId = examiner != 0 ? examiner : null;
                    practice.Begin = DateTime.Parse(practiceBegin);
                    practice.End = DateTime.Parse(practiceEnd);
                    if (id == 0)
                    {
                        db.Practices.Add(practice);
                        db.SaveChanges();
                        id = db.Cources.First(c => c.GroupId == group && c.Semester == semester && c.Year == year && c.Name == name).Id;
                        Group studentGroup = db.Groups.Include(g => g.Students).First(g => g.Id == practice.GroupId);
                        foreach (var student in studentGroup.Students)
                        {
                            db.Marks.Add(new Mark()
                            {
                                StudentId = student.Id,
                                CourceId = id,
                                ExamDate = (practice.FinalExamTime.HasValue ? practice.FinalExamTime.Value : DateTime.Now),
                                IsRetaken = false,
                                Point = 0
                            });
                        }
                    }
                    else
                    {
                        db.Practices.Update(practice);
                    }
                    db.SaveChanges();
                }
                else
                {
                    Cource cource = db.Cources.Find(id) ?? new Cource();
                    cource.Name = name;
                    cource.ShortName = shortName;
                    cource.FinalExamType = (ExamType)finalExamType;
                    if(finalExamTime != null)
                    {
                        cource.FinalExamTime = DateTime.Parse(finalExamTime);
                    }
                    cource.Semester = semester;
                    cource.Year = year;
                    float tmp = 0;
                    if(!float.TryParse(credits, out tmp))
                    {
                        float.TryParse(credits.Replace('.',','), out tmp);
                    }
                    cource.Credits = tmp;
                    cource.GroupId = group;
                    cource.TeacherId = teacher;
                    cource.ExaminerId = examiner != 0 ? examiner : null;
                    if (id == 0)
                    {
                        db.Cources.Add(cource);
                        db.SaveChanges();
                        id = db.Cources.First(c => c.GroupId == group && c.Semester == semester && c.Year == year && c.Name == name).Id;
                        Group studentGroup = db.Groups.Include(g => g.Students).First(g => g.Id == cource.GroupId);
                        foreach(var student in studentGroup.Students)
                        {
                            db.Marks.Add(new Mark()
                            {
                                StudentId = student.Id,
                                CourceId = id,
                                ExamDate = (cource.FinalExamTime.HasValue? cource.FinalExamTime.Value:DateTime.Now),
                                IsRetaken = false,
                                Point = 0
                            });
                        }
                    }
                    else
                    {
                        db.Cources.Update(cource);
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToPage("/Cources", new {id = 0});
        }


        public IActionResult OnGetDelete(int id)
        {
            if (!CurrentUserCanEditCources())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Cource cource = db.Cources.Find(id);
                db.Cources.Remove(cource);
                db.SaveChanges();
            }
            return RedirectToPage("/Cources", new { id = 0 });
        }

        public List<SelectListItem> GetTeacherOptions()
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
                    Selected = t.Id == cource.TeacherId,
                    Group = departments[t.DepartmentId]
                }).ToList();
            }
        }

        public List<SelectListItem> GetExaminerOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                Dictionary<int, SelectListGroup> departments = new Dictionary<int, SelectListGroup>();
                foreach (var item in db.Departments.ToList())
                {
                    departments.Add(item.Id, new SelectListGroup() { Name = item.ShortName==""?item.Name:item.ShortName });
                }
                return db.Teachers.OrderBy(t => t.Name).Select(t => new SelectListItem()
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == cource.ExaminerId,
                    Group = departments[t.DepartmentId]
                }).ToList();
            }
        } 

        public List<SelectListItem> GetGroupOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                Dictionary<int, SelectListGroup> specialties = new Dictionary<int, SelectListGroup>();
                foreach(var item in db.Specialties.ToList())
                {
                    specialties.Add(item.Id, new SelectListGroup() { Name = item.Id + " " + item.Name });
                }
                return db.Groups.Include(g => g.Specialty).Select(g => new SelectListItem()
                {
                    Value = g.Id.ToString(),
                    Text = g.GetCurrentGroupNumber().ToString(),
                    Group = specialties[g.SpecialtyId],
                    Selected = g.Id == cource.GroupId
                }).ToList().OrderBy(x=>x.Text).ToList();
            }
        }

        public List<SelectListItem> GetSemesterOptions(int current = 0)
        {
            if(cource != null)
            {
                return new List<SelectListItem>(new SelectListItem[] {
                new SelectListItem(){
                    Value = "1",
                    Text = "1",
                    Selected = cource.Semester == 1
                },
                 new SelectListItem() {
                    Value = "2",
                    Text = "2",
                    Selected = cource.Semester == 2
                    }
                });
            }
            else
            {
                return new List<SelectListItem>(new SelectListItem[] {
                new SelectListItem(){
                    Value = "1",
                    Text = "1",
                    Selected = current == 1
                },
                 new SelectListItem() {
                    Value = "2",
                    Text = "2",
                    Selected = current == 2
                    }
                });
            }
        }

        public List<SelectListItem> GetYearOptions(int current = 0)
        {
            var years = new List<SelectListItem>();
            if(cource != null)
            {
                for (int i = -6; i <= 3; i++)
                {
                    years.Add(new SelectListItem()
                    {
                        Value = (DateTime.Now.Year + i).ToString(),
                        Text = (DateTime.Now.Year + i).ToString(),
                        Selected = (DateTime.Now.Year + i) == cource.Year
                    });
                }
            }
            else
            {
                for (int i = -6; i <= 3; i++)
                {
                    years.Add(new SelectListItem()
                    {
                        Value = (DateTime.Now.Year + i).ToString(),
                        Text = (DateTime.Now.Year + i).ToString(),
                        Selected = (DateTime.Now.Year + i) == current
                    });
                }
            }
            return years;
        }

        public bool CurrentUserCanEditCources()
        {
            return GetCurrentUser().Role == Role.ADMIN || GetCurrentUser().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_COURCES);
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
