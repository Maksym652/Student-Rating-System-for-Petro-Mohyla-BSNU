namespace StudentRatingSystemWebApp.Models
{
    public class Cource
    {
        const int COUNT_OF_HOURS_IN_CREDIT = 30;
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ShortName { get; set; }
        public ExamType FinalExamType { get; set; }
        public DateTime? FinalExamTime { get; set; }
        public int Semester { get; set; }
        public int Year { get; set; }
        public float Credits { get; set; }
        public int? GroupId { get;set; }
        public Group? Group { get; set; }

        public int? ExaminerId { get; set; }
        /// <summary>
        /// The teacher who gives final grade on exam or credit.
        /// </summary>
        public Teacher? Examiner { get; set; }

        public int? TeacherId { get; set; }
        /// <summary>
        /// A teacher who conducted classes (seminars, lab works and practical works) and gave grades during the semester.
        /// </summary>
        public Teacher? Teacher { get; set; }

        public bool IsCurrent() => Semester == AcademicSemester.Now.SemesterNumber && Year == DateTime.Now.Year; 
        public AcademicSemester GetSemester() => new AcademicSemester(Semester, Semester == 1 ? Year : Year - 1);

        /// <summary>
        /// Returns semester number, counded from 1st studying year of the group. For example 2nd semester of 3rd studying year is 6th semester.
        /// </summary>
        /// <returns></returns>
        public int GetSemesterNumber() => (Group.GetCourceNumber(GetSemester()) - 1) * 2 + Semester;
        public int GetCourceNumber() => Group.GetCourceNumber(GetSemester());
        public int GetHoursCount() => (int)(Credits * COUNT_OF_HOURS_IN_CREDIT);
        public int GetGroupNumber() => Group.GetCourceNumber(GetSemester()) * 100 + (Group.Id % 100);

        public string GetFinalExamTypeName()
        {
            switch (FinalExamType)
            {
                case ExamType.ATTESTATION: return "атестація";
                case ExamType.CREDIT: return "залік";
                case ExamType.EXAM: return "іспит";
                case ExamType.PRACTICE: return "практика";
                default: return "";
            }
        }

        /// <summary>
        /// Returns short name of the cource if specified, otherwise returns abbreviation of the cource name.
        /// </summary>
        /// <returns>String that contains short name of the cource.</returns>
        public string GetAbbreviation()
		{
			if (ShortName is not null)
			{
                return ShortName;
			}
            string abbreviation = "";
            string prepositions = "і та з";
            foreach (var word in Name.Split(' ','-'))
			{
				if (!prepositions.Contains(word))
				{
                    abbreviation += word[0];
				}
			}
            return abbreviation.ToUpper();
		}

        public List<Mark> Marks { get; set; }
    }
}
