using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Controllers;

public class SavingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly FinanceService _finance;

    public SavingsController(AppDbContext db, FinanceService finance)
    {
        _db = db;
        _finance = finance;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var start = new DateTime(today.Year, today.Month, 1);
        var end = start.AddMonths(1);

        ViewBag.Dash = await _finance.GetDashboardAsync(today.Month, today.Year);

        var list = await _db.SavingsContributions
            .Where(s => s.Date >= start && s.Date < end)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        return View(list);
    }

    public IActionResult Create()
    {
        return View(new SavingsContribution());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Amount,Date,Description")] SavingsContribution model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.SavingsContributions.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Savings contribution recorded.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.SavingsContributions.FindAsync(id);
        if (item != null)
        {
            _db.SavingsContributions.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Contribution deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}