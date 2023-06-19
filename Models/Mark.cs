namespace StudentRatingSystemWebApp.Models
{
    public class Mark
    {
        public int Id { get; set; }
        public DateTime ExamDate { get; set; }
        public int Point { get; set; }

        /// <summary>
        /// If student had taking an exam more than 2 times before passed it, returns true.
        /// </summary>
        public bool IsRetaken { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourceId { get; set; }
        public Cource Cource { get; set; }

        /// <summary>
        /// If student's current rating point and scholarship depends on mark, returns true.
        /// </summary>
        public bool AffectsCurrentRating() => Cource.GetSemester() == AcademicSemester.PreviousSemester;

        /// <summary>
        /// If student successfully pass exam returns true.
        /// </summary>
        public bool IsCredited() => Point >= 60;

        public  ECTSpoint ConvertToECTS()
        {
            if(Point >= 90)
            {
                return ECTSpoint.A;
            }
            if(Point >= 85)
            {
                return ECTSpoint.B;
            }
            if(Point >= 75)
            {
                return ECTSpoint.C;
            }
            if(Point >= 70)
            {
                return ECTSpoint.D;
            }
            if(Point >= 60)
            {
                return ECTSpoint.E;
            }if(Point >= 35)
            {
                return ECTSpoint.FX;
            }
            return ECTSpoint.F;
        }

        public NationalScalePoint ConvertToNationalScale()
        {
            if(Cource.FinalExamType == ExamType.CREDIT)
			{
                return Point >= 60 ? NationalScalePoint.CREDITED : NationalScalePoint.NOT_CREDITED;
			}
            if (Point >= 90)
            {
                return NationalScalePoint.EXELENT;
            }
            if (Point >= 75)
            {
                return NationalScalePoint.GOOD;
            }
            if (Point >= 60)
            {
                return NationalScalePoint.FAIR;
            }
            return NationalScalePoint.POOR;
        }

        public string ConvertToNationalScaleAsString()
        {
            if (Cource.FinalExamType == ExamType.CREDIT)
            {
                return Point >= 60 ? "зарах." : "не зарах.";
            }
            if (Point >= 90)
            {
                return "відмінно";
            }
            if (Point >= 75)
            {
                return "добре";
            }
            if (Point >= 60)
            {
                return "задовільно";
            }
            return "незадовільно";
        }
    }
}
