using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;
using System.Security.Claims;

namespace StudentRatingSystemWebApp.Pages
{
    public class StatisticsModel : PageModel
    {
        public List<StatisticsData> stats = new();
        public List<SelectListItem> specialties;
        public void OnGet(int specialtyId=0)
        {
            List<Student> students;
            using (var db = new StudentRatingDbContext())
            {
                if(specialtyId == 0)
                {
                    students = db.Students
                    .Include(s => s.Group).ThenInclude(g => g.Specialty)
                    .Include(s => s.AdditionalPoints)
                    .Include(s => s.Scholarship)
                    .Include(s => s.Marks).ThenInclude(m => m.Cource)
                    .ToList();
                }
                else
                {
                    students = db.Students
                   .Include(s => s.Group).ThenInclude(g => g.Specialty)
                   .Include(s => s.AdditionalPoints)
                   .Include(s => s.Scholarship)
                   .Include(s => s.Marks).ThenInclude(m => m.Cource)
                   .Where(s => s.Group.SpecialtyId == specialtyId)
                   .ToList();
                }
                specialties = new List<SelectListItem>();
                specialties.Add(new SelectListItem() { Value="0", Text="Всі спеціальності", Selected=specialtyId==0});
                specialties.AddRange(
                    db.Specialties.Include(s => s.Groups).Where(s => s.Groups.Count > 0)
                    .Select(s => new SelectListItem()
                    {
                        Value = s.Id.ToString(),
                        Text = s.Id.ToString() + " " + s.Name,
                        Selected = s.Id == specialtyId
                    }).ToList()
                );
            }
            AcademicSemester semester = AcademicSemester.Now-10;
            for (int i = 0; i < 10; i++)
            {
                stats.Add(new StatisticsData());
                stats[i] = GetStatsForSemester(students, semester);
                semester += 1;
            }
        }

        private StatisticsData GetStatsForSemester(List<Student> students, AcademicSemester semester)
        {
            StatisticsData stats = new StatisticsData();
            if (students.Where(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester))>0).Count() != 0)
            {
                stats.AveragePoint = students
                    .Where(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester)) > 0)
                    .Average(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester)));
            }
            stats.SumOfAdditionalPoints = students.Sum(s => s.AdditionalPoints.Where(a => a.GetSemester() == semester).Sum(a => a.Point));
            stats.SumOfAdditionalPointsForPublicActivity = students.Sum(s => s.AdditionalPoints.Where(a => a.GetSemester() == semester && a.Type == AdditionalPointType.PUBLIC_ACTIVITY).Sum(a => a.Point));
            stats.SumOfAdditionalPointsForScientificWork = students.Sum(s => s.AdditionalPoints.Where(a => a.GetSemester() == semester && a.Type == AdditionalPointType.SCIENTIFIC_WORK).Sum(a => a.Point));
            stats.SumOfAdditionalPoitnsForSportAchievments = students.Sum(s => s.AdditionalPoints.Where(a => a.GetSemester() == semester && a.Type == AdditionalPointType.SPORT_ACHIEVEMENTS).Sum(a => a.Point));
            for (int cource = 1; cource <= 6; cource++)
            {
                if (students.Where(s => s.Group.GetCourceNumber(semester) == cource).Where(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester)) > 0).Count() > 0)
                {
                    stats.AveragePointsOnCources.Add(
                        students
                        .Where(s => s.Group.GetCourceNumber(semester) == cource)
                        .Where(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester)) > 0)
                        .Average(s => s.CalculateWeightedAveragePointForSemester(s.Group.GetSemesterNumber(semester)))
                    );
                }
                else
                {
                    stats.AveragePointsOnCources.Add(0);
                }
            }
            return stats;
        }
    }
}
