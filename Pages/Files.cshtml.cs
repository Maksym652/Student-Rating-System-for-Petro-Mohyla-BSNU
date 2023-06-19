using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp.Pages
{
    public class FilesModel : PageModel
    {
        public List<string> documents;
        public List<string> regulations;

        public List<SelectListItem> selectListItems = new List<SelectListItem>()
        {
            new SelectListItem()
            {
                Value="documents",
                Text="документи для ознайомлення"
            },
            new SelectListItem()
            {
                Value="regulations",
                Text="правила і положення"
            }
        };

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public FilesModel(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _environment = env;
        }
        public void OnGet()
        {
            ReadFileNames();
        }

        public IActionResult OnPostFile(string type, IFormFile file, string filename)
        {
            if (!CurrentUserCanManageFiles())
            {
                return Forbid();
            }
            if (file != null)
            {
                if (filename == "" || filename == null)
                {
                    filename = file.FileName;
                }
                else
                {
                    filename += "." + file.FileName.Split('.').Last();
                }
                string path = Path.Combine(_environment.WebRootPath, "files/"+type, filename);
                using (var filestream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(filestream);
                }
            }
            ReadFileNames();
            return null;
        }

        public IActionResult OnGetDelete(string file, string folder)
        {
            if (!CurrentUserCanManageFiles())
            {
                return Forbid();
            }
            string path = Path.Combine(_environment.WebRootPath, "files", folder, file);
            System.IO.File.Delete(path);
            ReadFileNames();
            return null;
        }

        private void ReadFileNames()
        {
            documents = new List<string>();
            regulations = new List<string>();
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "files\\documents")))
            {
                documents.Add(file.Replace(_environment.WebRootPath + "\\", ""));
            }
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "files\\regulations")))
            {
                regulations.Add(file.Replace(_environment.WebRootPath + "\\", ""));
            }
        }

        public bool CurrentUserCanManageFiles()
        {
            return GetCurrentUser().Role == Role.ADMIN || GetCurrentUser().Permissions.Any(p => p.Id == (int)Permissions.MANAGE_FILES);
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
