using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VBJWeboldal.Data;
using VBJWeboldal.Models;

namespace VBJWeboldal.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("/esemenyek")]
        public async Task<IActionResult> Index()
        {
            // --- AUTOMATIKUS TÖRLÉS LOGIKA (Lusta törlés) ---
            var pastEvents = await _context.Events.Where(e => e.EventDate.Date < DateTime.Now.Date).ToListAsync();
            if (pastEvents.Any())
            {
                _context.Events.RemoveRange(pastEvents);
                await _context.SaveChangesAsync();
            }
            var upcomingEvents = await _context.Events
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return View(upcomingEvents);
        }
    }
}