namespace StudentRatingSystemWebApp.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        public int HeadOfDepartmentId { get; set; }

        public Teacher? GetHeadOfDepartment()
		{
            return Teachers.Find(t => t.Id == HeadOfDepartmentId)??null;
		}

        public List<Teacher> Teachers { get; set; }
    }
}
