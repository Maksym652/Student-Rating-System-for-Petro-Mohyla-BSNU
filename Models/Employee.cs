using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRatingSystemWebApp.Models
{
    [Table("Employees")]
    public class Employee : User
    {
        public string Post { get; set; }
    }
}
