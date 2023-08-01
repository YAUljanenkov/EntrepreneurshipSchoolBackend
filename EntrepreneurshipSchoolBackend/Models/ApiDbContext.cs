using EntrepreneurshipSchoolBackend.Utility;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EntrepreneurshipSchoolBackend.Models;
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options): base(options)
    {
        this.Database?.EnsureCreated();
    }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<AssessmentsType> AssessmentsTypes { get; set; }
    public DbSet<Assessments> Assessments { get; set; }
    public DbSet<Attend> Attends { get; set; }
    public DbSet<Claim> Claim { get; set; }
    public DbSet<ClaimStatus> ClaimStatuses { get; set; }
    public DbSet<ClaimType> ClaimTypes { get; set; }
    public DbSet<FinalGradeType> FinalTypes { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Learner> Learner { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Lot> Lots { get; set; }
    public DbSet<Relate> Relates { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskType> TaskTypes { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<UserFile> UserFiles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
