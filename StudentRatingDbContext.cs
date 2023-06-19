using Microsoft.EntityFrameworkCore;
using StudentRatingSystemWebApp.Models;

namespace StudentRatingSystemWebApp
{
    public class StudentRatingDbContext : DbContext
    {
        public DbSet<AdditionalPoint> AdditionalPoints { get; set; } = null!;
        public DbSet<Cource> Cources { get; set; } = null!;
        public DbSet<Department> Departments  { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Faculty> Faculties  { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Mark> Marks  { get; set; } = null!;
        public DbSet<Permission> Permissions  { get; set; } = null!;
        public DbSet<Practice> Practices { get; set; }=null!;
        public DbSet<ScholarshipType> ScholarshipTypes { get; set; } = null!;
        public DbSet<Specialty> Specialties  { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public StudentRatingDbContext() => Database.EnsureCreated();//creates db if it not exist

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(u => u.Password).HasDefaultValue(new HashCalculator().CalculateHash("12345678"));
            modelBuilder.Entity<User>().Property(u => u.Contacts).HasDefaultValue("");
            modelBuilder.Entity<Specialty>().Property(s => s.PointScale).HasDefaultValue(PointScale.ONE_HUNDRED_POINT_SCALE);
            modelBuilder.Entity<Student>().Property(s => s.Role).HasDefaultValue(Role.STUDENT);
            modelBuilder.Entity<Teacher>().Property(t => t.Role).HasDefaultValue(Role.TEACHER);
            modelBuilder.Entity<Employee>().Property(a => a.Role).HasDefaultValue(Role.UNIVERSITY_EMPLOYEE);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=StudentRatingDb;Trusted_Connection=True;");
        }
    }
}
