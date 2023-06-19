namespace StudentRatingSystemWebApp.Models
{
    public class AdditionalPoint
    {
        public int Id { get; set; }
        public AdditionalPointType Type { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public float Point { get; set; }
        public string? ConfirmationFileName { get; set; }

        public bool HasConfirmationDocument()
        {
            if(ConfirmationFileName is null)
            {
                return false;
            }
            return ConfirmationFileName != "";
        }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public string GetTypeName()
        {
            switch (Type)
            {
                case AdditionalPointType.PUBLIC_ACTIVITY: return "громадська діяльність";
                case AdditionalPointType.SCIENTIFIC_WORK: return "наукова робота";
                case AdditionalPointType.SPORT_ACHIEVEMENTS: return "спортивні досягнення";
                default: return "";
            }
        }

        public string GetDefaultConfirmationFileName()
        {
            return Student.Name + "_" + Date.ToString("dd-MM-yyyy") + "_" + Point.ToString() + "б_" + (Description.Length > 100 ? Description.Substring(0, 100) : Description);
        }

        public int GetSemesterNumber()
		{
            return Student.Group.GetSemesterNumberByDate(Date);
		}

        public AcademicSemester GetSemester()
        {
            return AcademicSemester.GetByDate(Date);
        }

        public bool AffectsCurrentRating()
        {
            return AcademicSemester.GetByDate(Date) == AcademicSemester.PreviousSemester;
        }

    }
}
