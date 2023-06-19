using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class UsersModel : PageModel
    {
        public List<Student> students;
        public List<Teacher> teachers;
        public List<Employee> employees;
        public User? editedUser;
        public List<SelectListItem> options;

        public void OnGet()
        {
            using(var db = new StudentRatingDbContext())
            {
                students = db.Students.Include(s => s.Group).ThenInclude(g => g.Specialty).OrderBy(s => s.Name).OrderByDescending(s => s.GroupId).ToList();
                teachers = db.Teachers.Include(t => t.Department).OrderBy(t => t.Name).OrderBy(t => t.Department.Name).ToList();
                employees = db.Employees.OrderBy(e => e.Name).ToList();
            }
        }

        public IActionResult OnGetEdit(int id, int role)
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            if(id == 0)
            {
                switch (role)
                {
                    case (int)Role.STUDENT: editedUser = new Student() { Id=0, Name="", GroupId=0, IsOnBudget=false, Login="", Role=Role.STUDENT, StudentNumber=0 }; break; 
                    case (int)Role.TEACHER: editedUser = new Teacher() { Id = 0, Name = "", Post = "", AcademicDegree="", Login = "", Role = Role.TEACHER, DepartmentId = 0 }; break;
                    case (int)Role.UNIVERSITY_EMPLOYEE: editedUser = new Employee() { Id = 0, Name = "", Login = "", Role = Role.UNIVERSITY_EMPLOYEE, Post = "" }; break;
                    case (int)Role.ADMIN: editedUser = new Employee() { Id = 0, Name = "", Login = "", Role = Role.ADMIN, Post = "" }; break;
                    default: editedUser = new User(); break;
                }
            }
            using(var db = new StudentRatingDbContext())
            {
                if(id != 0)
                {
                    switch (role)
                    {
                        case (int)Role.STUDENT: editedUser = db.Students.Find(id); break;
                        case (int)Role.TEACHER: editedUser = db.Teachers.Find(id); break;
                        case (int)Role.UNIVERSITY_EMPLOYEE: editedUser = db.Employees.Find(id); break;
                        case (int)Role.ADMIN: editedUser = db.Employees.Find(id); break;
                        default: editedUser = db.Users.Find(id); break;
                    }
                }
                if(editedUser is Student)
                {
                    options = GetGroupOptions();
                }
                if (editedUser is Teacher)
                {
                    Dictionary<int, SelectListGroup> faculties = new Dictionary<int, SelectListGroup>();
                    foreach(var item in db.Faculties.OrderBy(f => f.Name).ToList())
                    {
                        faculties.Add(item.Id, new SelectListGroup() { Name = item.ShortName});
                    }
                    options = db.Departments.Include(d => d.Faculty).OrderBy(d => d.Name).Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name,
                        Selected = d.Id == ((Teacher)editedUser).DepartmentId,
                        Group = faculties[d.FacultyId]
                    }).ToList();
                }
            }
            return null;
        }

        private List<SelectListItem> GetGroupOptions()
        {
            using (var db = new StudentRatingDbContext())
            {
                Dictionary<int, SelectListGroup> specialties = new Dictionary<int, SelectListGroup>();
                foreach (var item in db.Specialties.ToList())
                {
                    specialties.Add(item.Id, new SelectListGroup() { Name = item.Id + " " + item.Name });
                }
                return db.Groups.Include(g => g.Specialty).Select(g => new SelectListItem()
                {
                    Value = g.Id.ToString(),
                    Text = g.GetCurrentGroupNumber().ToString(),
                    Group = specialties[g.SpecialtyId],
                    Selected = g.Id == (editedUser as Student).GroupId
                }).ToList().OrderBy(x => x.Text).ToList();
            }
        }

        public IActionResult OnPostStudent(int id, string name, int number, int group, int isOnBudget, string login = "")
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Student student = db.Students.Find(id)??new Student() { Login=login};
                student.Name = name;
                student.StudentNumber = number;
                student.GroupId = group;
                student.IsOnBudget = isOnBudget == 1;
                student.Role = Role.STUDENT;
                if(id == 0)
                {
                    db.Students.Add(student);
                }
                else
                {
                    db.Students.Update(student);
                }
                db.SaveChanges();
                if(id == 0)
                {
                    id = db.Students.Where(s => s.StudentNumber == student.StudentNumber).First().Id;
                }
            }
            return RedirectToPage("/Profile", new { id = id }); ;
        }

        public IActionResult OnPostTeacher(int id, string name, string degree, string post, int department, string login = "")
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Teacher teacher = db.Teachers.Find(id)??new Teacher() { Login = login };
                teacher.Name = name;
                teacher.AcademicDegree = degree;
                teacher.Post = post;
                teacher.DepartmentId = department;
                teacher.Role = Role.TEACHER;
                if (id == 0)
                {
                    db.Teachers.Add(teacher);
                }
                else
                {
                    db.Teachers.Update(teacher);
                }
                db.SaveChanges();
                if (id == 0)
                {
                    id = db.Teachers.Where(t => t.Name == name && t.AcademicDegree == degree && t.Post == post).First().Id;
                }
            }
            return RedirectToPage("/Profile", new { id = id }); ;
        }

        public IActionResult OnPostEmployee(int id, string name,string post,string login)
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Employee employee = db.Employees.Find(id)??new Employee() { Login=login};
                employee.Name = name;
                employee.Post = post;
                employee.Role = Role.UNIVERSITY_EMPLOYEE;
                if (id == 0)
                {
                    db.Employees.Add(employee);
                }
                else
                {
                    db.Employees.Update(employee);
                }
                db.SaveChanges();
                if (id == 0)
                {
                    id = db.Employees.Where(e => e.Name == name && e.Post == post).First().Id;
                }
            }
            return RedirectToPage("/Profile", new { id = id }); ;
        }
        public IActionResult OnPostAdmin(int id, string name, string post, string login)
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            using (var db = new StudentRatingDbContext())
            {
                Employee employee = db.Employees.Find(id) ?? new Employee() { Login = login };
                employee.Name = name;
                employee.Post = post;
                employee.Role = Role.ADMIN;
                if (id == 0)
                {
                    db.Employees.Add(employee);
                }
                else
                {
                    db.Employees.Update(employee);
                }
                db.SaveChanges();
                if (id == 0)
                {
                    id = db.Employees.Where(e => e.Name == name && e.Post == post).First().Id;
                }
            }
            return RedirectToPage("/Profile", new { id = id }); ;
        }

        public IActionResult OnPostDelete(int id)
        {
            if (!CurrentUserCanEditUsers())
            {
                return Forbid();
            }
            using(var db = new StudentRatingDbContext())
            {
                User user = db.Users.Find(id);
                db.Users.Remove(user);
                db.SaveChanges();
                students = db.Students.Include(s => s.Group).ThenInclude(g => g.Specialty).ToList();
                students.Sort((x, y) => x.CompareWithoutRatingPoint(x, y));
                teachers = db.Teachers.Include(t => t.Department).ToList();
                teachers.Sort();
                employees = db.Employees.Where(u => u.Role == Role.UNIVERSITY_EMPLOYEE || u.Role == Role.ADMIN).ToList();
                employees.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            return null;
        }

        public bool CurrentUserCanEditUsers()
        {
            return GetCurrent().Role == Role.ADMIN || GetCurrent().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_USERS);
        }
        private User GetCurrent()
        {
            using(var db = new StudentRatingDbContext())
            {
                return db.Users.Include(u => u.Permissions).First(u => u.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value));
            }
        }
    }
}
