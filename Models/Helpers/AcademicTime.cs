namespace StudentRatingSystemWebApp
{
    /// <summary>
    /// Describes semester (period of time which is a half of studying year).
    /// </summary>
    public class AcademicSemester
    {
        /// <summary>
        /// Number of the semester, could be 1 (from September to December) or 2 (from January to June)
        /// </summary>
        public int SemesterNumber { get; set; }
        /// <summary>
        /// Year when the studying year began.
        /// </summary>
        public int BeginYear { get; set; }
        /// <summary>
        /// Year when the studying year ended.
        /// </summary>
        public int EndYear { get => BeginYear + 1; }

        public static AcademicSemester Now { get => GetCurrent(); }
        public static AcademicSemester PreviousSemester { get =>  new AcademicSemester(DateTime.Now.Month >= 9 ? 2 : 1, DateTime.Now.Year-1); }
        public static AcademicSemester NextSemester { get =>  new AcademicSemester(DateTime.Now.Month >= 9 ? 2 : 1, DateTime.Now.Year); }

        public AcademicSemester Previous { get => SemesterNumber == 1 ? new AcademicSemester(2, BeginYear - 1) : new AcademicSemester(1, BeginYear); }
        public AcademicSemester Next { get => SemesterNumber == 1 ? new AcademicSemester(2, BeginYear) : new AcademicSemester(1, BeginYear + 1); }

        public AcademicSemester(int semester, int beginYear)
        {
            SemesterNumber = semester;
            BeginYear = beginYear;
        }

        private static AcademicSemester GetCurrent()
        {
            int semester = DateTime.Now.Month >= 9 ? 1 : 2;
            int beginYear = DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year-1;
            return new AcademicSemester(semester, beginYear);
        }
        public static AcademicSemester GetByDate(DateTime date)
        {
            int semester = date.Month >= 9 ? 1 : 2;
            int beginYear = date.Month >= 9 ? date.Year : date.Year - 1;
            return new AcademicSemester(semester, beginYear);
        }

        public override bool Equals(object? obj)
        {
            return obj is AcademicSemester semester &&
                   SemesterNumber == semester.SemesterNumber &&
                   BeginYear == semester.BeginYear;
        }

        public static bool operator ==(AcademicSemester obj1, AcademicSemester obj2)
        {
            return obj1.Equals(obj2);
        }
        public static bool operator !=(AcademicSemester obj1, AcademicSemester obj2)
        {
            return !obj1.Equals(obj2);
        }
        public static AcademicSemester operator +(AcademicSemester at, int i)
        {
            AcademicSemester result = at;
            while(i != 0)
            {
                result = result.Next;
                i--;
            }
            return result;
        }
        public static AcademicSemester operator -(AcademicSemester at, int i)
        {
            AcademicSemester result = at;
            while (i != 0)
            {
                result = result.Previous;
                i--;
            }
            return result;
        }

        public override string ToString()
        {
            return  BeginYear + "/" + (EndYear-2000)+" "+SemesterNumber;
        }
    }
}
