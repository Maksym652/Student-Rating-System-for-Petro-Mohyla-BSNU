using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRatingSystemWebApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public Role Role { get; set; }
        public string Password { get; set; }
        public string Contacts { get; set; }

        public List<Permission> Permissions { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>String containing familyname and initials</returns>
        public string GetShorterName()
		{
            string familyname = Name.Split(' ')[0];
            string firstName = Name.Split(' ')[1];
            string patronimicName = Name.Split(' ')[2];
            return familyname + " " + firstName[0]+ "." + patronimicName[0]+".";
		}
    }
}
