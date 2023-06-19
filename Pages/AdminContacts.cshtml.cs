using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class AdminContactsModel : PageModel
    {
        public List<Employee> admins { get; set; }
        public void OnGet()
        {
            using (var db = new StudentRatingDbContext())
            {
                admins = db.Employees.Where(e => e.Role == Role.ADMIN).ToList();
            }
        }
    }
}
