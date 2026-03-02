using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VBJWeboldal.Data;
using VBJWeboldal.Models;
using VBJWeboldal.ViewModels;



namespace VBJWeboldal.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Konstruktor frissítése az adatbázis befogadására:
    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        // 1. Hírek lekérése
        var publishedNews = await _context.News.Where(n => n.IsPublished).OrderByDescending(n => n.PublishedAt).Take(3).ToListAsync();

        // 2. Galériák lekérése
        var galleries = await _context.Galleries.Include(g => g.Images).OrderByDescending(g => g.Id).Take(3).ToListAsync();

        // 3. Legújabb PUBLIKUS dokumentumok lekérése (MAX 3 DARAB)
        var latestDocs = await _context.Documents
            .Where(d => d.IsPublic)
            .OrderByDescending(d => d.UploadedAt)
            .Take(3)
            .ToListAsync();
        // 4. Közelgõ események
        var upcomingEvents = await _context.Events
                                           .Where(e => e.EventDate >= DateTime.Now)
                                           .OrderBy(e => e.EventDate)
                                           .Take(3)
                                           .ToListAsync();
        //közelgõ események rész
        // --- AUTOMATIKUS TÖRLÉS LOGIKA (Lusta törlés) ---
        var pastEvents = await _context.Events.Where(e => e.EventDate.Date < DateTime.Now.Date).ToListAsync();
        if (pastEvents.Any())
        {
            _context.Events.RemoveRange(pastEvents);
            await _context.SaveChangesAsync();
        }



        var viewModel = new VBJWeboldal.ViewModels.HomeViewModel
        {
            NewsList = publishedNews,
            Galleries = galleries,
            LatestDocuments = latestDocs, // <--- Ezt adjuk át
            UpcomingEvents = upcomingEvents,

            HirekSzama = await _context.News.CountAsync(n => n.IsPublished),
            GaleriaKepekSzama = await _context.Galleries.SelectMany(g => g.Images).CountAsync(),
            EsemenyekSzama = await _context.Events.CountAsync(e => e.EventDate >= DateTime.Now)
            
        };


        return View(viewModel);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [Route("/kapcsolat")]
    public async Task<IActionResult> Contact()
    {
        //Lekérjük a három engedélyezett szerepkör tagjait
        var editors = await _userManager.GetUsersInRoleAsync("Editor");
        var readers = await _userManager.GetUsersInRoleAsync("Reader");
        var galleryManagers = await _userManager.GetUsersInRoleAsync("GalleryManager");

        //Összevonjuk õket, és kiszûrjük a duplikációkat
        var allStaff = editors.Concat(readers).Concat(galleryManagers)
                              .GroupBy(u => u.Id)
                              .Select(g => g.First())
                              .ToList();

        //Lekérjük az Adminokat, hogy el tudjuk rejteni õket
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = admins.Select(a => a.Id).ToHashSet();

        //Kiszûrjük az Adminokat, és név szerint sorba rendezzük a maradékot
        var displayStaff = allStaff
            .Where(u => !adminIds.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .ToList();

        //Átadjuk a kész listát a HTML nézetnek
        return View(displayStaff);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
