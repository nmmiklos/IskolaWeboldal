using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;

namespace VBJWeboldal.Controllers
{
    [Route("galeria")]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Az összes galéria (album) listázása
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Lekérjük az albumokat, a hozzájuk tartozó képekkel együtt
            var galleries = await _context.Galleries
                .Include(g => g.Images)
                .ToListAsync();

            return View(galleries);
        }

        //Egy konkrét galéria megtekintése (benne a képekkel)
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            // Megkeressük az adott ID-jú albumot
            var gallery = await _context.Galleries
                .Include(g => g.Images)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gallery == null) return NotFound();

            return View(gallery);
        }
    }
}