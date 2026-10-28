using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Controllers;

public class PlanController : Controller
{
    private readonly AppDbContext _db;

    public PlanController(AppDbContext db)
    {
        _db = db;
    }

    // Shows the plan form for a month (this month by default)
    public async Task<IActionResult> Index(int? month, int? year)
    {
        int m = month ?? DateTime.Today.Month;
        int y = year ?? DateTime.Today.Year;

        var plan = await _db.MonthlyPlans.FirstOrDefaultAsync(p => p.Month == m && p.Year == y)
                   ?? new MonthlyPlan { Month = m, Year = y };

        return View(plan);
    }

    // Saves the plan: creates it, or updates it if that month already has one
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        [Bind("Month,Year,IncomeType,MonthlyIncome,SavingsGoalType,SavingsGoalValue")] MonthlyPlan model)
    {
        // Savings goal must not exceed available income (PRD FR-01)
        if (model.SavingsGoalType == SavingsGoalType.Percentage && model.SavingsGoalValue > 100)
        {
            ModelState.AddModelError(nameof(model.SavingsGoalValue), "Percentage cannot be more than 100.");
        }
        if (model.SavingsGoalType == SavingsGoalType.FixedAmount && model.SavingsGoalValue > model.MonthlyIncome)
        {
            ModelState.AddModelError(nameof(model.SavingsGoalValue), "Savings goal cannot be more than your income.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _db.MonthlyPlans
            .FirstOrDefaultAsync(p => p.Month == model.Month && p.Year == model.Year);

        if (existing == null)
        {
            _db.MonthlyPlans.Add(model);
        }
        else
        {
            existing.IncomeType = model.IncomeType;
            existing.MonthlyIncome = model.MonthlyIncome;
            existing.SavingsGoalType = model.SavingsGoalType;
            existing.SavingsGoalValue = model.SavingsGoalValue;
        }

        await _db.SaveChangesAsync();

        decimal planned = FinanceService.CalculatePlannedSavings(
            model.MonthlyIncome, model.SavingsGoalType, model.SavingsGoalValue);
        decimal spendable = model.MonthlyIncome - planned;
        var inr = new CultureInfo("en-IN");

        TempData["Success"] = $"Plan saved. Planned savings: ₹{planned.ToString("N2", inr)}, " +
                              $"spendable budget: ₹{spendable.ToString("N2", inr)}.";

        return RedirectToAction(nameof(Index), new { month = model.Month, year = model.Year });
    }
}