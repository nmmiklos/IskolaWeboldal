using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Data;
using VBJWeboldal.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace VBJWeboldal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Bővített konstruktor
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News.Include(n => n.Author).OrderByDescending(n => n.PublishedAt).ToListAsync();
            return View(newsList);
        }

        // --- ÚJ BEJEGYZÉS (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Editor")]
        public IActionResult CreateNews()
        {
            // Egy üres modelt adunk át, hogy a View tudja: ez egy új bejegyzés (Id == 0)
            return View(new News());
        }

        // --- ÚJ BEJEGYZÉS MENTÉSE (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> CreateNews(News model, IFormFile? coverImage)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                model.AuthorId = currentUser.Id;
                model.PublishedAt = DateTime.Now;

                // Kép feltöltés logikája
                if (coverImage != null && coverImage.Length > 0)
                {
                    model.CoverImagePath = await UploadImageAsync(coverImage);
                }

                _context.News.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // --- SZERKESZTÉS (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> EditNews(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            // Ugyanazt a CreateNews.cshtml fájlt használjuk!
            return View("CreateNews", news);
        }

        // --- SZERKESZTÉS MENTÉSE (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> EditNews(int id, News model, IFormFile? coverImage)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingNews = await _context.News.FindAsync(id);
                if (existingNews == null) return NotFound();

                // Frissítjük a szöveges adatokat
                existingNews.Title = model.Title;
                existingNews.Content = model.Content;
                existingNews.IsPublished = model.IsPublished;

                // Ha töltött fel új képet, lecseréljük a régit
                if (coverImage != null && coverImage.Length > 0)
                {
                    existingNews.CoverImagePath = await UploadImageAsync(coverImage);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View("CreateNews", model);
        }

        // SEGÉDFÜGGVÉNY A KÉPFELTÖLTÉSHEZ
        private async Task<string> UploadImageAsync(IFormFile file)
        {
            // wwwroot/uploads mappa létrehozása, ha nem létezik
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Egyedi fájlnév generálása, hogy ne írják felül egymást (pl. Guid + eredeti név SRC: Stackoverflow:https://stackoverflow.com/questions/1602578/c-what-is-the-fastest-way-to-generate-a-unique-filename)
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Ezt az útvonalat mentjük az adatbázisba (webes formátum)
            return "/uploads/" + uniqueFileName;
        }
        // --- BEJEGYZÉS TÖRLÉSE (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            // Megkeressük a törlendő bejegyzést az adatbázisban
            var news = await _context.News.FindAsync(id);

            if (news != null)
            {
                // 1. Töröljük a fizikai képet a szerverről, ha volt hozzá feltöltve
                if (!string.IsNullOrEmpty(news.CoverImagePath))
                {
                    // A "/uploads/fajlnev.jpg" webes útvonalból csinálunk fizikai útvonalat (C:\...\wwwroot\uploads\...)
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.CoverImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // 2. Töröljük magát a bejegyzést az adatbázisból
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }

            // Visszadobjuk a felhasználót az Irányítópultra
            return RedirectToAction("Index");
        }

        // --- FELHASZNÁLÓK LISTÁZÁSA ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // --- ÚJ FELHASZNÁLÓ LÉTREHOZÁSA (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        // --- ÚJ FELHASZNÁLÓ LÉTREHOZÁSA (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(VBJWeboldal.ViewModels.CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Jogosultság hozzárendelése (Admin, Editor vagy Reader)
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // FONTOS: Nem léptetjük be az új usert (nincs SignInAsync), 
                    // így az Admin marad bejelentkezve!
                    return RedirectToAction("Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        public IActionResult Images()
        {
            return Content("Ide jön majd a képek feltöltése és a galéria kezelő!");
        }
    }
}