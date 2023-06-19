namespace StudentRatingSystemWebApp.Models
{
    public class SemesterStats
    {
        public StatisticsData UniversityStats = new();
        public List<Faculty> Faculties = new();
        public List<StatisticsData> FacutiesStats = new();
        public List<Specialty> Specialties = new();
        public List<StatisticsData> SpecialtiesStats = new();
    }
}
