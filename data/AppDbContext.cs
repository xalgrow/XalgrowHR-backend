using Microsoft.EntityFrameworkCore;
using XalgrowHR.Models;

namespace XalgrowHR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet for Users, Employees, and Departments
        public DbSet<User> Users { get; set; } // For managing users
        public DbSet<Employee> Employees { get; set; } // For managing employees
        public DbSet<Department> Departments { get; set; } // For managing departments

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define optional (nullable) one-to-many relationship between Employee and Department
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .IsRequired(false); // Make DepartmentId nullable

            base.OnModelCreating(modelBuilder);
        }
    }
}
