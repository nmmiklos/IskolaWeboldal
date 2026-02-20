using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VBJWeboldal.Models;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;
using VBJWeboldal.ViewModels;



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
        var model = new HomeViewModel
        {
            // hírek
            FrissHirek = await _context.News
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.PublishedAt)
                .Take(4)
                .ToListAsync(),

            //események
            KozelgoEsemeny = await _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync(),

            // Galérrai
            FrissGaleriaKep = await _context.Galleries
                .OrderByDescending(g => g.Id)
                .Take(6)
                .ToListAsync(),

            // szamlalok
            HirekSzama = await _context.News
                .Where(n => n.IsPublished)
                .CountAsync(),

            EsemenyekSzama = await _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .CountAsync(),

            GaleriaKepekSzama = await _context.Galleries
                .CountAsync()
        };

        return View(model);
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
