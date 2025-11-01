using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookHubo.Models;

namespace BookHubo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BookHubDbContext _context;

    public HomeController(ILogger<HomeController> logger, BookHubDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get 12 newest active books
        var recentBooks = await _context.Books
            .Include(b => b.Seller)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .Take(12)
            .ToListAsync();

        return View(recentBooks);
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
