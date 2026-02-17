using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;
using VBJWeboldal.Models;

namespace VBJWeboldal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Irányítópult betöltése a bejegyzésekkel
        public async Task<IActionResult> Index()
        {
            // Lekérjük az összes bejegyzést, szerzővel együtt, legújabbal kezdve
            var newsList = await _context.News.Include(n => n.Author).OrderByDescending(n => n.PublishedAt).ToListAsync();
            return View(newsList);
        }

        // 2. Új bejegyzés űrlap (GET)
        [HttpGet]
        public IActionResult CreateNews()
        {
            return View();
        }

        // 3. Új bejegyzés mentése (POST)
        [HttpPost]
        public async Task<IActionResult> CreateNews(News model)
        {
            if (ModelState.IsValid)
            {
                // Hozzárendeljük a bejelentkezett felhasználót szerzőként
                var currentUser = await _userManager.GetUserAsync(User);
                model.AuthorId = currentUser.Id;
                model.PublishedAt = DateTime.Now;

                _context.News.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index"); // Vissza az irányítópultra
            }
            return View(model);
        }

        // Ideiglenes oldal a képeknek, hogy a link ne törjön el
        public IActionResult Images()
        {
            return Content("Ide jön majd a képek feltöltése és a galéria kezelő!");
        }
    }
}