using SpendWise.Models;

namespace SpendWise.ViewModels;

public class CategoryTotal
{
    public ExpenseCategory Category { get; set; }
    public decimal Total { get; set; }
    public decimal Percent { get; set; }
}

public class DashboardViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }

    public bool HasPlan { get; set; }
    public MonthlyPlan? Plan { get; set; }

    public decimal MonthlyIncome { get; set; }
    public decimal PlannedSavings { get; set; }
    public decimal ActualSavings { get; set; }
    public decimal RemainingSavingsGoal { get; set; }
    public decimal SavingsProgressPercent { get; set; }

    public decimal SpendableBudget { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal TodaySpending { get; set; }
    public int RemainingDays { get; set; }
    public decimal SuggestedDailyAllowance { get; set; }

    public bool BudgetExceeded { get; set; }
    public bool BudgetWarning { get; set; }
    public bool OverDailyAllowance { get; set; }
    public bool SavingsGoalReached { get; set; }

    public List<CategoryTotal> CategoryTotals { get; set; } = new();
    public List<Expense> RecentExpenses { get; set; } = new();
}