using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.ViewModels;

namespace SpendWise.Services;

public class FinanceService
{
    private readonly AppDbContext _db;

    public FinanceService(AppDbContext db)
    {
        _db = db;
    }

    // Rule 1: planned savings from fixed amount or percentage
    public static decimal CalculatePlannedSavings(decimal income, SavingsGoalType type, decimal value)
    {
        return type == SavingsGoalType.Percentage
            ? Math.Round(income * value / 100m, 2)
            : value;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(int month, int year)
    {
        var vm = new DashboardViewModel { Month = month, Year = year };

        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var plan = await _db.MonthlyPlans
            .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);

        vm.Plan = plan;
        vm.HasPlan = plan != null;

        // Rule 3: actual savings come from recorded contributions
        vm.ActualSavings = await _db.SavingsContributions
            .Where(s => s.Date >= start && s.Date < end)
            .SumAsync(s => s.Amount);

        // Expenses for the selected month (Rules 5 and 8)
        var monthExpenses = _db.Expenses.Where(e => e.Date >= start && e.Date < end);
        vm.TotalExpenses = await monthExpenses.SumAsync(e => e.Amount);

        // Rule 7: today's spending
        vm.TodaySpending = await _db.Expenses
            .Where(e => e.Date >= today && e.Date < tomorrow)
            .SumAsync(e => e.Amount);

        // Category breakdown
        var grouped = await monthExpenses
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        vm.CategoryTotals = grouped
            .OrderByDescending(g => g.Total)
            .Select(g => new CategoryTotal
            {
                Category = g.Category,
                Total = g.Total,
                Percent = vm.TotalExpenses > 0 ? Math.Round(g.Total / vm.TotalExpenses * 100m, 1) : 0
            })
            .ToList();

        // Recent transactions
        vm.RecentExpenses = await monthExpenses
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .Take(5)
            .ToListAsync();

        if (plan == null)
        {
            return vm; // no plan yet: the dashboard shows an empty state
        }

        // Rules 1 and 2
        vm.MonthlyIncome = plan.MonthlyIncome;
        vm.PlannedSavings = CalculatePlannedSavings(plan.MonthlyIncome, plan.SavingsGoalType, plan.SavingsGoalValue);
        vm.SpendableBudget = plan.MonthlyIncome - vm.PlannedSavings;

        // Rule 4: remaining savings goal (never below zero), no divide-by-zero
        vm.RemainingSavingsGoal = Math.Max(0, vm.PlannedSavings - vm.ActualSavings);
        vm.SavingsProgressPercent = vm.PlannedSavings > 0
            ? Math.Round(vm.ActualSavings / vm.PlannedSavings * 100m, 2)
            : 0;

        // Rule 5
        vm.RemainingBudget = vm.SpendableBudget - vm.TotalExpenses;

        // Rule 6: remaining days depend on which month is selected
        int daysInMonth = DateTime.DaysInMonth(year, month);
        bool isCurrentMonth = year == today.Year && month == today.Month;

        if (isCurrentMonth) vm.RemainingDays = daysInMonth - today.Day + 1;
        else if (start > today) vm.RemainingDays = daysInMonth;   // future month
        else vm.RemainingDays = 0;                                // past month

        vm.SuggestedDailyAllowance = vm.RemainingDays > 0
            ? Math.Round(Math.Max(0, vm.RemainingBudget) / vm.RemainingDays, 2)
            : 0;

        // Status flags (PRD FR-06)
        vm.BudgetExceeded = vm.RemainingBudget < 0;
        vm.BudgetWarning = !vm.BudgetExceeded
            && vm.SpendableBudget > 0
            && vm.TotalExpenses / vm.SpendableBudget >= 0.8m;

        // Today's allowance is measured from the start of today, so the
        // money already spent today does not shrink the limit it is compared to.
        if (isCurrentMonth && vm.RemainingDays > 0)
        {
            decimal allowanceAtStartOfDay = Math.Max(0, vm.RemainingBudget + vm.TodaySpending) / vm.RemainingDays;
            vm.OverDailyAllowance = vm.TodaySpending > allowanceAtStartOfDay;
        }

        vm.SavingsGoalReached = vm.PlannedSavings > 0 && vm.ActualSavings >= vm.PlannedSavings;

        return vm;
    }
}