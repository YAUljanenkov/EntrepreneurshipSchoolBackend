using EntrepreneurshipSchoolBackend.Utility;
using Microsoft.EntityFrameworkCore;
using System;

namespace EntrepreneurshipSchoolBackend.Models;
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options): base(options)
    {
    }
    
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

    public void CheckAndCreate(IType obj, double weight)
    {
        if (obj.GetType() == new TransactionType().GetType())
        {
            TransactionType? found = TransactionTypes.FirstOrDefault(rec => obj.Name == rec.Name);

            if(found != null) {
                return;
            }

            found = new TransactionType();
            found.Id = obj.Id;
            found.Name = obj.Name;

            TransactionTypes.Add(found);
        }

        if (obj.GetType() == new TaskType().GetType())
        {
            TaskType? found = TaskTypes.FirstOrDefault(rec => obj.Name == rec.Name);

            if (found != null)
            {
                return;
            }

            found = new TaskType();
            found.Id = obj.Id;
            found.Name = obj.Name;

            TaskTypes.Add(found);
        }

        if (obj.GetType() == new ClaimType().GetType())
        {
            ClaimType? found = ClaimTypes.FirstOrDefault(rec => obj.Name == rec.Name);

            if(found != null)
            {
                return;
            }

            found = new ClaimType();
            found.Id = obj.Id;
            found.Name = obj.Name;

            ClaimTypes.Add(found);
        }

        if (obj.GetType() == new AssessmentsType().GetType())
        {
            AssessmentsType? found = AssessmentsTypes.FirstOrDefault(rec => obj.Name == rec.Name);

            if(found != null)
            {
                return;
            }

            found = new AssessmentsType();
            found.Id = obj.Id;
            found.Name = obj.Name;

            AssessmentsTypes.Add(found);
        }

        if (obj.GetType() == new FinalGradeType().GetType())
        {
            FinalGradeType? found = FinalTypes.FirstOrDefault(rec => obj.Name == rec.Name);

            if (found != null)
            {
                return;
            }

            found = new FinalGradeType();
            found.Id = obj.Id;
            found.Name = obj.Name;
            found.Weight = weight;

            FinalTypes.Add(found);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
