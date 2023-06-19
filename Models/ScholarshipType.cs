using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRatingSystemWebApp.Models
{
    public class ScholarshipType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public List<Student> Students { get; set; } = new();
    }
}