using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;
using VBJWeboldal.ViewModels;
using VBJWeboldal.Models;

namespace VBJWeboldal.Controllers
{
    // Megadjuk az alap útvonalat
    [Route("Hirek")]
    public class HirekController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HirekController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Elérhető: /Hirek vagy /Hirek/Index
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var hirekListaja = await _context.News // Itt a te táblanevedet (News) használtam
                .Where(h => h.IsPublished == true)
                .OrderByDescending(h => h.PublishedAt)
                .ToListAsync();

            var viewModel = new NewsListViewModel
            {
                News = hirekListaja
            };

            return View(viewModel);
        }

        // Elérhető: /Hirek/Details/5
        // Fontos: Itt kötelezővé tesszük az ID-t a route-ban, így nem keveredik az Index-szel
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            // Javított lekérdezés a piros hullámvonal ellen:
            var cikk = await _context.News.FirstOrDefaultAsync(m => m.Id == id);

            if (cikk == null)
            {
                return NotFound();
            }
            var viewModel = new NewsDetailsViewModel
            {
                HirItem = cikk
            };
            return View(viewModel);
        }
    }
}