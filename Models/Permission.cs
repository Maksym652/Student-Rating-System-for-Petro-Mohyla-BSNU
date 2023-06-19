namespace StudentRatingSystemWebApp.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<User> Users { get; set; } = new();

        public override bool Equals(object? obj)
        {
            return obj is Permission permission &&
                   Id == permission.Id;
        }
    }
}
