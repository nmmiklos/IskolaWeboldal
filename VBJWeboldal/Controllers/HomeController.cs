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
        //Publikált Hírek lekérése (legutóbbi 4)
        var publishedNews = await _context.News
            .Where(n => n.IsPublished)
            .OrderByDescending(n => n.PublishedAt)
            .Take(4)
            .ToListAsync();

        //Galériák lekérése a borítóképekkel (legutóbbi 5)
        var galleries = await _context.Galleries
            .Include(g => g.Images)
            .OrderByDescending(g => g.Id)
            .Take(5)
            .ToListAsync();

        //Összecsomagolás a ViewModelbe
        var viewModel = new VBJWeboldal.ViewModels.HomeViewModel
        {
            NewsList = publishedNews,
            Galleries = galleries
        };

        return View(viewModel);
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
