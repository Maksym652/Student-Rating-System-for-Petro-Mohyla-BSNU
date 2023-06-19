using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRatingSystemWebApp.Models
{
    public class Group
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        /// <summary>
        /// Form of studying such as fulltime or external.
        /// </summary>
        public EducationForm EducationForm { get; set; }

        public int SpecialtyId { get; set; }
        public Specialty Specialty { get; set; }

        public int? CouratorId { get; set; }
        public Teacher? Courator { get; set; }

        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }

        public List<Student> Students { get; set; } = new();

        public List<Cource> Cources { get; set; } = new();

        public int GetCurrentGroupNumber()
        {
            return (Id % 100) + (GetCurrentCource() * 100);
        }
        public int GetGroupNumber(int semester)
        {
            return (Id % 100) + ((1+semester)/2) * 100;
        }
        public int GetCurrentCource()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            int currentCource = currentYear - GetAdmissionYear();
            currentCource += ((Id % 1000) / 100)-1;
            if(currentMonth >= 9)
            {
                currentCource++;
            }
            return currentCource;
        }

        public int GetCourceNumber(AcademicSemester semester)
		{
            return semester.BeginYear - GetAdmissionYear() + 1;
        }

        /// <summary>
        /// Returns number of current semester counted from beginning of 1st studying year. For example 2nd semester of 3rd studying year is 6th semester.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSemesterNumber()
		{
            return (GetCurrentCource() - 1) * 2 + AcademicSemester.Now.SemesterNumber;
		}

        public int GetSemesterNumberByDate(DateTime date)
		{
            AcademicSemester semester = AcademicSemester.GetByDate(date);
            return GetSemesterNumber(semester);
        }
        public int GetSemesterNumber(AcademicSemester semester)
        {
            return (semester.BeginYear - GetAdmissionYear()) * 2 + semester.SemesterNumber;
        }
        private int GetAdmissionYear()
        {
            return ((Id / 1000) % 100)+2000;
        }

        public string GetEducationForm()
        {
            switch (EducationForm)
            {
                case EducationForm.FULL_TIME_DAY: return "денна";
                case EducationForm.FULL_TIME_EVENING: return "вечірня";
                case EducationForm.EXTERNAL: return "заочна";
                default: return "не вказано";
            }
        }

        public string GetEducationLevel()
        {
            if(GetCurrentCource() <= 4)
            {
                return "бакалавр";
            }
            else
            {
                return "магістр";
            }
        }
    }
}
