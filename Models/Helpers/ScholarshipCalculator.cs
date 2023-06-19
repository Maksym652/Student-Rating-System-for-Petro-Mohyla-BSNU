namespace StudentRatingSystemWebApp.Models
{
    public class ScholarshipCalculator
    {
        /// <summary>
        /// Percent students of particular cource and speciality with budget form of education, who get academic scholarship.
        /// </summary>
        public int AcademicScholarshipsRate = 60;

        public List<Student> GetListOfAcademicScholarships(List<Student> students)
        {
            if(!HasSameCourceAndSpeciality(students))
            {
                return new List<Student>();
            }
            students = students.Where(s => s.IsOnBudget).ToList();
            students.Sort();
            return students.GetRange(0, (int)Math.Floor(students.Count * (AcademicScholarshipsRate/100f)));
        }
        public List<Student> GetListOfAcademicScholarships(List<Student> students, int semesterNumber)
        {
            if (!HasSameCourceAndSpeciality(students))
            {
                return new List<Student>();
            }
            students = students.Where(s => s.IsOnBudget && s.CountOfAcademicArrears(semesterNumber) == 0 && s.CountOfRetakenExams(semesterNumber) == 0).ToList();
            students.Sort((x,y)=>x.CompareToInSemester(y,semesterNumber));
            return students.GetRange(0, (int)Math.Floor(students.Count * (AcademicScholarshipsRate / 100f)));
        }
        private static bool HasSameCourceAndSpeciality(List<Student> students)
        {
            return students.All(s => students.First().Group.Specialty == s.Group.Specialty && students.First().Group.GetCurrentCource() == s.Group.GetCurrentCource());
        }
    }
}
