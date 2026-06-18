using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<ProjectRequest> ProjectRequests => Set<ProjectRequest>();
    public DbSet<AiProjectAnalysis> AiProjectAnalyses => Set<AiProjectAnalysis>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Employee>().Property(x => x.HourlyRate).HasConversion<double>();
        builder.Entity<Employee>().HasMany(x => x.Skills).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectRequest>().HasOne(x => x.Analysis).WithOne(x => x.ProjectRequest).HasForeignKey<AiProjectAnalysis>(x => x.ProjectRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectRequest>().HasOne(x => x.ClientUser).WithMany().HasForeignKey(x => x.ClientUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
