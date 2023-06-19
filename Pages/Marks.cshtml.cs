using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class MarksModel : PageModel
    {
        public List<List<Mark>> sessions;
        public List<Mark> practice;
        public Student student;
        public IActionResult OnGet(int id = 0)
        {
            using(var db = new StudentRatingDbContext())
            {
                User user = db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
                if (user.Role == Role.STUDENT)
                {
                    GetData(user.Id);
                }else if(user.Permissions.Any(p => p.Id == (int)Permissions.VIEW_ALL_POINTS) || user.Role == Role.ADMIN){
                    GetData(id);
                }else if(user.Role == Role.TEACHER)
                {
                    Teacher teacher = db.Teachers.Include(t => t.CurratedGroup).First(t => t.Id == user.Id);
                    Student student = db.Students.Find(id);
                    if(student.GroupId == teacher.CurratedGroup.Id)
                    {
                        GetData(id);
                    }
                }
                else
                {
                    return Forbid();
                }
                if(student is null)
                {
                    return NotFound();
                }
                return null;
            }
        }

        private void GetData(int studentId)
        {
            using(var db = new StudentRatingDbContext())
            {
                student = db.Students
                        .Include(s => s.Group).ThenInclude(g => g.Specialty)
                        .Include(s => s.Group).ThenInclude(g => g.Faculty)
                        .First(s => s.Id == studentId);
                List<Mark> marks = db.Marks
                    .Include(m => m.Cource).ThenInclude(c => c.Group)
                    .Include(m => m.Cource).ThenInclude(c => c.Examiner)
                    .Where(m => m.StudentId == student.Id && m.Point > 0).ToList();
                int currentSemester = student.Group.GetCurrentSemesterNumber();
                sessions = new List<List<Mark>>(currentSemester - 1);
                for (int semester = 1; semester < currentSemester; semester++)
                {
                    sessions.Add(marks.Where(m => m.Cource.GetSemesterNumber() == semester && m.Cource.FinalExamType != ExamType.PRACTICE).ToList());
                    sessions[semester - 1].Sort((x, y) => DateTime.Compare(x.ExamDate, y.ExamDate));
                }
                practice = marks.Where(m => m.Cource.FinalExamType == ExamType.PRACTICE).ToList();
            }
        }
    }
}
