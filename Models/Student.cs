using System.ComponentModel.DataAnnotations.Schema;
namespace StudentRatingSystemWebApp.Models
{
    [Table("Students")]
    public class Student : User, IComparable, IComparer<Student>
    {
        public int StudentNumber { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public bool IsOnBudget { get; set; }
        public int ScholarshipTypeId { get; set; }
        public ScholarshipType Scholarship { get; set; }

        public List<Mark> Marks { get; set; }
        public List<AdditionalPoint> AdditionalPoints { get; set; }
        
        public float CalculateRatingPoint()
        {
            List<Mark> marks = Marks.ToList().Where(x => x.AffectsCurrentRating()).ToList();
            float sumOfAdditionalPoints = AdditionalPoints.Where(x => AcademicSemester.GetByDate(x.Date)==AcademicSemester.PreviousSemester && x.HasConfirmationDocument()).Sum(x => x.Point);
            if (marks.Any(mark => !mark.IsCredited() || mark.IsRetaken))
            {
                return 0;
            }
            marks.RemoveAll(x => x.Cource.FinalExamType == ExamType.ATTESTATION);
            return CalculateRatingPoint(CalculateWeightedAveragePoint(marks), sumOfAdditionalPoints);
        }
        public float CalculateRatingPointForSemester(int semesterNumber)
        {
            List<Mark> marks = Marks.Where(x => x.Cource.GetSemesterNumber() == semesterNumber).ToList();
            float sumOfAdditionalPoints = AdditionalPoints.Where(x => x.GetSemesterNumber() == semesterNumber && x.HasConfirmationDocument()).Sum(x => x.Point);
            if (marks.Any(mark => !mark.IsCredited() || mark.IsRetaken))
            {
                return 0;
            }
            marks.RemoveAll(x => x.Cource.FinalExamType == ExamType.ATTESTATION);
            return CalculateRatingPoint(CalculateWeightedAveragePoint(marks), sumOfAdditionalPoints);
        }

        public float CalculateWeightedAveragePoint()
        {
            return CalculateWeightedAveragePoint(Marks.Where(x => x.AffectsCurrentRating() && x.Cource.FinalExamType != ExamType.ATTESTATION).ToList());
        }
        public float CalculateWeightedAveragePoint(List<Mark> marks)
        {
            if(marks.Count == 0)
            {
                return 0;
            }
            float sumOfCredits = marks.Sum(x => x.Cource.Credits);
            float ratingPoint = 0;
            foreach (var mark in marks)
            {
                ratingPoint += mark.Point * mark.Cource.Credits / sumOfCredits;
            }
            return ratingPoint;
        }
        public float CalculateWeightedAveragePointForSemester(int semesterNumber)
		{
            return CalculateWeightedAveragePoint(Marks.Where(x => x.Cource.GetSemesterNumber() == semesterNumber && x.Cource.FinalExamType != ExamType.ATTESTATION).ToList());
        }

        private float CalculateRatingPoint(float weightedAveragePoint , float sumOfAdditionalPoints)
        {
            return weightedAveragePoint * 0.9f + sumOfAdditionalPoints;
        }

        public int CountOfAcademicArrears() => Marks.Where(x => x.Point < 60).Count();
        public int CountOfAcademicArrears(int semesterNumber) => Marks.Where(x => x.Cource.GetSemesterNumber()<=semesterNumber && x.Point < 60).Count();
        public int CountOfRetakenExams() => Marks.Where(x => x.AffectsCurrentRating() && x.IsRetaken).Count();
        public int CountOfRetakenExams(int semesterNumber) => Marks.Where(x => x.Cource.GetSemesterNumber() == semesterNumber && x.IsRetaken).Count();
        public bool IsExcellentStudent() => Marks.Where(x => x.AffectsCurrentRating()).Count()>0 && Marks.Where(x => x.AffectsCurrentRating()).All(x => x.Point >= 90);
        public bool WasExcellentStudent(int semesterNumber) => Marks.Where(x => x.Cource.GetSemesterNumber() == semesterNumber).Count()>0 && Marks.Where(x => x.Cource.GetSemesterNumber() == semesterNumber).All(x => x.Point >= 90);

        public int GetCource() => Group.GetCurrentCource();

        public Specialty GetSpecialty() => Group.Specialty;

        public int CompareTo(object? obj)
        {
            Student st2 = obj as Student;
            if(st2 == null)
            {
                return -1;
            }
            if(CountOfAcademicArrears() != st2.CountOfAcademicArrears())
            {
                return CountOfAcademicArrears() - st2.CountOfAcademicArrears();
            }
            if (IsOnBudget != st2.IsOnBudget)
            {
                return IsOnBudget ? -1 : 1;
            }
            return (int)(st2.CalculateRatingPoint()*100 - CalculateRatingPoint()*100);

        }
        public int CompareToInSemester(object? obj, int semesterNumber)
        {
            Student st2 = obj as Student;
            if (st2 == null)
            {
                return -1;
            }
            if (CountOfAcademicArrears(semesterNumber) != st2.CountOfAcademicArrears(semesterNumber))
            {
                return CountOfAcademicArrears(semesterNumber) - st2.CountOfAcademicArrears(semesterNumber);
            }
            if (IsOnBudget != st2.IsOnBudget)
            {
                return IsOnBudget ? -1 : 1;
            }
            return (int)(st2.CalculateRatingPointForSemester(semesterNumber) * 100 - CalculateRatingPointForSemester(semesterNumber) * 100);

        }
        public int Compare(Student? x, Student? y)
        {
            return x.CompareTo(y);
        }
        public int CompareInSemester(Student? x, Student? y, int semesterNumber)
		{
            return x.CompareToInSemester(y, semesterNumber);
		}


        public int CompareWithoutRatingPoint(Student? x, Student? y)
        {
            if(x.GroupId == y.GroupId)
            {
                return x.Name.CompareTo(y.Name);
            }
            return x.GroupId.CompareTo(y.GroupId);
        }

        public override bool Equals(object? obj)
        {
            return obj is Student student &&
                   StudentNumber == student.StudentNumber;
        }
    }
}
