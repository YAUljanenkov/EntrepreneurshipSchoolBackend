using Microsoft.EntityFrameworkCore;
using System;

namespace EntrepreneurshipSchoolBackend.Models;
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options): base(options)
    {

    }
    public DbSet<Lesson> Lessons { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}