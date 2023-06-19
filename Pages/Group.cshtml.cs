using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class GroupModel : PageModel
    {
        public Group? group;
        public IActionResult OnGet(int id=0)
        {
            using (var db = new StudentRatingDbContext())
            {
                if(id == 0)
                {
                    if (PageContext.HttpContext.User.IsInRole("STUDENT"))
                    {
                        id = db.Students
                            .First(x => x.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value)).GroupId;
                    }
                    if (PageContext.HttpContext.User.IsInRole("TEACHER"))
                    {
                        Group? group = db.Teachers.Include(t => t.CurratedGroup)
                            .First(x => x.Id == int.Parse(PageContext.HttpContext.User.FindFirst("ID").Value)).CurratedGroup;
                        if(group is null)
                        {
                            return NotFound();
                        }
                        id = group.Id;
                    }
                }
                group = db.Groups
                    .Include(g => g.Students.OrderBy(s => s.Name)).ThenInclude(s => s.Marks).ThenInclude(m => m.Cource)
                    .Include(g => g.Students.OrderBy(s => s.Name)).ThenInclude(s => s.AdditionalPoints)
                    .Include(g => g.Students.OrderBy(s => s.Name)).ThenInclude(s => s.Scholarship)
                    .Include(g => g.Specialty)
                    .Include(g => g.Faculty)
                    .FirstOrDefault(g => g.Id == id);
                if(group is null)
                {
                    return NotFound();
                }
                return null;
            }
        }
    }
}
