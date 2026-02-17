using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VBJWeboldal.Models;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;

namespace VBJWeboldal.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    // Konstruktor frissítése az adatbázis befogadására:
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        // Csak a publikált bejegyzéseket szedjük ki!
        var publishedNews = await _context.News
            .Where(n => n.IsPublished)
            .OrderByDescending(n => n.PublishedAt)
            .Take(4) // Csak a legutóbbi 4-et mutatjuk
            .ToListAsync();

        return View(publishedNews);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("/kapcsolat")]
    public IActionResult Contact()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
