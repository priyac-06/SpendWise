using Microsoft.EntityFrameworkCore;
using SpendWise.Models;

namespace SpendWise.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MonthlyPlan> MonthlyPlans => Set<MonthlyPlan>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<SavingsContribution> SavingsContributions => Set<SavingsContribution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PRD 7.3: no duplicate plans for the same month and year
        modelBuilder.Entity<MonthlyPlan>()
            .HasIndex(p => new { p.Month, p.Year })
            .IsUnique();
    }
}