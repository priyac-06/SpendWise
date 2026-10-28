using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Controllers;

public class HomeController : Controller
{
    private readonly FinanceService _finance;

    public HomeController(FinanceService finance)
    {
        _finance = finance;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var vm = await _finance.GetDashboardAsync(today.Month, today.Year);
        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}