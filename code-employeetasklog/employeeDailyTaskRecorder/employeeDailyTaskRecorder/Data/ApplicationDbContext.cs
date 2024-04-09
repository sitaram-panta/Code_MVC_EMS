using employeeDailyTaskRecorder.Models;
using Microsoft.EntityFrameworkCore;
namespace employeeDailyTaskRecorder.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<LeaveRequest> leaveRequest { get; set; }
        public DbSet<EmployeeMigration> mig_Employees { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasMany(a => a.Records)
                .WithOne(b => b.Employee)
                .HasForeignKey(b => b.EmployeeId);
        }
      
    }
}
