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
        var upcomingEvents = await _context.Events.Where(e => e.EventDate >= DateTime.Now).OrderBy(e => e.EventDate).Take(2).ToListAsync();
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
        // 1. Lekérjük az ÖSSZES érintett szerepkört (most már az Adminokat is idevesszük a listába)
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var editors = await _userManager.GetUsersInRoleAsync("Editor");
        var readers = await _userManager.GetUsersInRoleAsync("Reader");
        var galleryManagers = await _userManager.GetUsersInRoleAsync("GalleryManager");

        // 2. Összevonjuk õket, és kiszûrjük a duplikációkat
        var allStaff = admins.Concat(editors).Concat(readers).Concat(galleryManagers)
                              .GroupBy(u => u.Id)
                              .Select(g => g.First())
                              .ToList();

        // 3. Eltároljuk az Adminok ID-ját, hogy tudjuk, kik õk
        var adminIds = admins.Select(a => a.Id).ToHashSet();

        // 4. AZ ÚJ SZÛRÉSI LOGIKA:
        // - Ha a felhasználó NEM admin -> mindenképp listázzuk.
        // - Ha a felhasználó ADMIN -> CSAK AKKOR listázzuk, ha van kitöltött Titulusa (!string.IsNullOrWhiteSpace)
        var displayStaff = allStaff
            .Where(u => !adminIds.Contains(u.Id) || !string.IsNullOrWhiteSpace(u.Title))
            .OrderBy(u => u.FullName)
            .ToList();

        // Átadjuk a kész listát a HTML nézetnek
        return View(displayStaff);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [Route("/szak/{id}")]
    public IActionResult Szak(string id)
    {
        var szakok = new Dictionary<string, SzakViewModel>()
        {
            ["gepeszet"] = new SzakViewModel
            {
                Name = "Gépészet",
                Description = "A gépészet ágazatban modern technológiákkal és ipari ismeretekkel ismerkednek meg a diákok.",
                HeroClass = "hero-gepeszet",
                HeroImageClass = "hero-illustration-gepeszet",
                 PartialName = "Gepeszet"
            },

            ["informatika"] = new SzakViewModel
            {
                Name = "Informatika és távközlés",
                Description = "Programozás, hálózatok és modern IT rendszerek tanulása.",
                HeroClass = "hero-info",
                HeroImageClass = "hero-illustration-info",
                PartialName = "Informatika"
            },

            ["elektro"] = new SzakViewModel
            {
                Name = "Elektronika és elektrotechnika",
                Description = "Elektronikai rendszerek, áramkörök és modern technológia.",
                HeroClass = "hero-elektro",
                HeroImageClass = "hero-illustration-elektro",
                PartialName = "Elektrotechnika"
            },

            ["kozg"] = new SzakViewModel
            {
                Name = "Gazdálkodás és menedzsment",
                Description = "Gazdasági és üzleti alapismeretek modern környezetben.",
                HeroClass = "hero-kozg",
                HeroImageClass = "hero-illustration-kozg",
                PartialName = "Kozgazdalkodas"
            },

            ["gimi"] = new SzakViewModel
            {
                Name = "Gimnázium",
                Description = "Általános mûveltséget és továbbtanulási lehetõséget biztosító képzés.",
                HeroClass = "hero-gimi",
                HeroImageClass = "hero-illustration-gimi",
                PartialName = "Gimnazium"
            }
        };

        if (!szakok.ContainsKey(id))
            return NotFound();

        return View("Szak", szakok[id]);
    }
}
