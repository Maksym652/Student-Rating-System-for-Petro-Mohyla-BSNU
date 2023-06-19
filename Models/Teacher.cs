using System.ComponentModel.DataAnnotations.Schema;
namespace StudentRatingSystemWebApp.Models
{
    [Table("Teachers")]
    public class Teacher : User, IComparable, IComparer<Student>
    {
        public string? Post { get; set; }
        public string? AcademicDegree { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public Group? CurratedGroup { get; set; }

        public bool IsHeadOfDepartment()
        {
            return Department.HeadOfDepartmentId == Id;
        }

        public bool IsDeanOfFaculty()
        {
            return Department.Faculty.DeanId == Id;
        }

        List<Cource> Cources { get; set; } = new();

        public int Compare(Student? x, Student? y)
        {
            return x.CompareTo(y);
        }

        public int CompareTo(object? obj)
        {
            Teacher t = obj as Teacher;
            if (t == null)
            {
                return -1;
            }
            if(DepartmentId == t.DepartmentId)
            {
                return Name.CompareTo(t.Name);
            }
            return DepartmentId - t.DepartmentId;
        }
    }
}
