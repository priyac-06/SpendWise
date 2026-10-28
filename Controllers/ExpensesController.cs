using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;

namespace SpendWise.Controllers;

public class ExpensesController : Controller
{
    private readonly AppDbContext _db;

    public ExpensesController(AppDbContext db)
    {
        _db = db;
    }

    // List with optional filters (date range and category)
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, ExpenseCategory? category)
    {
        var query = _db.Expenses.AsQueryable();

        if (from.HasValue) query = query.Where(e => e.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(e => e.Date < to.Value.Date.AddDays(1));
        if (category.HasValue) query = query.Where(e => e.Category == category.Value);

        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.SelectedCategory = category.HasValue ? ((int)category.Value).ToString() : "";
        ViewBag.Total = await query.SumAsync(e => e.Amount);

        var list = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .ToListAsync();

        return View(list);
    }

    public IActionResult Create()
    {
        return View(new Expense());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Amount,Category,Date,Description")] Expense model)
    {
        if (!Enum.IsDefined(model.Category))
        {
            ModelState.AddModelError(nameof(model.Category), "Please choose a valid category.");
        }

        if (!ModelState.IsValid) return View(model);

        _db.Expenses.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Expense added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Title,Amount,Category,Date,Description")] Expense model)
    {
        if (id != model.Id) return NotFound();

        if (!Enum.IsDefined(model.Category))
        {
            ModelState.AddModelError(nameof(model.Category), "Please choose a valid category.");
        }

        if (!ModelState.IsValid) return View(model);

        var existing = await _db.Expenses.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = model.Title;
        existing.Amount = model.Amount;
        existing.Category = model.Category;
        existing.Date = model.Date;
        existing.Description = model.Description;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Expense updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense != null)
        {
            _db.Expenses.Remove(expense);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Expense deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
