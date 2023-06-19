using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRatingSystemWebApp.Models
{
    public class Specialty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }

        public PointScale PointScale { get; set; }
        public List<Group> Groups { get; set; } = new();
    }
}
