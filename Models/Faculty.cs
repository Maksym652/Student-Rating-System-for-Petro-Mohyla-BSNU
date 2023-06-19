namespace StudentRatingSystemWebApp.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public int? DeanId { get; set; }
        public Teacher? GetDeanOfFaculty()
		{
            List<Teacher> teachers = new List<Teacher>();
            foreach(var dep in Departments)
			{
                teachers.AddRange(dep.Teachers);
			}
            return teachers.FirstOrDefault(t => t.Id == DeanId);
		}

        public List<Department> Departments { get; set; } = new();

        public List<Group> Groups { get; set; } = new();
    }
}
