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
        // ÚJ BEJEGYZÉS LÉTREHOZÁSA (POST)
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> CreateNews(News model, IFormFile? coverImage)
        {
            // --- DOKUMENTUM LINK BIZTONSÁGI ELLENŐRZÉSE ---
            if (!string.IsNullOrWhiteSpace(model.AttachedDocumentUrl))
            {
                string relativePath = model.AttachedDocumentUrl;
                if (relativePath.Contains("/uploads/documents/"))
                {
                    relativePath = relativePath.Substring(relativePath.IndexOf("/uploads/documents/"));
                }

                var attachedDoc = await _context.Documents.FirstOrDefaultAsync(d => d.FilePath == relativePath);

                if (attachedDoc == null)
                {
                    ModelState.AddModelError("AttachedDocumentUrl", "❌ Hiba: A megadott dokumentum nem található a rendszerben!");
                }
                else if (!attachedDoc.IsPublic)
                {
                    ModelState.AddModelError("AttachedDocumentUrl", "🔒 Biztonsági hiba: Ez egy BELSŐ (tanári) dokumentum, nem csatolható publikus hírhez!");
                }
                else
                {
                    model.AttachedDocumentUrl = relativePath;
                }
            }
            // --- ELLENŐRZÉS VÉGE ---

            if (ModelState.IsValid)
            {
                // Képfeltöltés logikája
                if (coverImage != null && coverImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(coverImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }
                    model.CoverImagePath = "/uploads/" + uniqueFileName;
                }

                model.PublishedAt = DateTime.Now;
                _context.News.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        // BEJEGYZÉS SZERKESZTÉSE (POST)
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> EditNews(int id, News model, IFormFile? coverImage)
        {
            // --- DOKUMENTUM LINK BIZTONSÁGI ELLENŐRZÉSE ---
            if (!string.IsNullOrWhiteSpace(model.AttachedDocumentUrl))
            {
                string relativePath = model.AttachedDocumentUrl;
                if (relativePath.Contains("/uploads/documents/"))
                {
                    relativePath = relativePath.Substring(relativePath.IndexOf("/uploads/documents/"));
                }

                var attachedDoc = await _context.Documents.FirstOrDefaultAsync(d => d.FilePath == relativePath);

                if (attachedDoc == null)
                {
                    ModelState.AddModelError("AttachedDocumentUrl", "❌ Hiba: A megadott dokumentum nem található a rendszerben!");
                }
                else if (!attachedDoc.IsPublic)
                {
                    ModelState.AddModelError("AttachedDocumentUrl", "🔒 Biztonsági hiba: Ez egy BELSŐ (tanári) dokumentum, nem csatolható publikus hírhez!");
                }
                else
                {
                    model.AttachedDocumentUrl = relativePath;
                }
            }
            // --- ELLENŐRZÉS VÉGE ---

            if (ModelState.IsValid)
            {
                var existingNews = await _context.News.FindAsync(id);
                if (existingNews == null) return NotFound();

                existingNews.Title = model.Title;
                existingNews.Content = model.Content;
                existingNews.IsPublished = model.IsPublished;
                existingNews.AttachedDocumentUrl = model.AttachedDocumentUrl;

                // Új kép feltöltése esetén felülírjuk a régit
                if (coverImage != null && coverImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(coverImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }
                    existingNews.CoverImagePath = "/uploads/" + uniqueFileName;
                }

                _context.News.Update(existingNews);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
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

        //Galériák listázása
        [HttpGet]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> Galleries()
        {
            // Lekérjük a galériákat, és azt is, hány kép van bennük
            var galleries = await _context.Galleries.Include(g => g.Images).ToListAsync();
            return View(galleries);
        }

        //Új Galéria (Album) létrehozása
        [HttpPost]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> CreateGallery(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var gallery = new Gallery { Title = title };
                _context.Galleries.Add(gallery);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Galleries");
        }

        //Galéria részletei (Képek listája és feltöltés)
        [HttpGet]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> GalleryDetails(int id)
        {
            var gallery = await _context.Galleries.Include(g => g.Images).FirstOrDefaultAsync(g => g.Id == id);
            if (gallery == null) return NotFound();

            return View(gallery);
        }

        //Képek feltöltése a Galériába
        [HttpPost]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> UploadImages(int galleryId, List<IFormFile> files)
        {
            var gallery = await _context.Galleries.FindAsync(galleryId);
            if (gallery == null) return NotFound();

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "gallery");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Kép mentése az adatbázisba
                    var image = new Image
                    {
                        ImagePath = "/uploads/gallery/" + uniqueFileName,
                        GalleryId = galleryId
                    };
                    _context.Images.Add(image);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("GalleryDetails", new { id = galleryId });
        }

        //Galéria törlése (képekkel együtt)
        [HttpPost]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> DeleteGallery(int id)
        {
            var gallery = await _context.Galleries.Include(g => g.Images).FirstOrDefaultAsync(g => g.Id == id);
            if (gallery != null)
            {
                // Fizikai fájlok törlése
                foreach (var img in gallery.Images)
                {
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, img.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.Galleries.Remove(gallery);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Galleries");
        }

        //Egyetlen kép törlése a Galériából
        [HttpPost]
        [Authorize(Roles = "Admin,GalleryManager")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            // Megkeressük a képet az adatbázisban
            var image = await _context.Images.FindAsync(id);
            if (image == null) return NotFound();

            int galleryId = image.GalleryId; // Eltároljuk az album ID-t a visszairányításhoz

            //Fizikai fájl törlése a szerverről
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            //Kép törlése az adatbázisból
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            // Visszadobjuk a felhasználót ugyanannak az albumnak a szerkesztőjébe
            return RedirectToAction("GalleryDetails", new { id = galleryId });
        }


        //Dokumentumok listázása (Minden bejelentkezett, Reader is látja)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Documents()
        {
            var docs = await _context.Documents.OrderByDescending(d => d.UploadedAt).ToListAsync();
            return View(docs);
        }

        //Új dokumentum feltöltése (Csak Admin és Editor)
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> UploadDocument(string title, bool isPublic, IFormFile file)
        {
            if (file != null && file.Length > 0 && !string.IsNullOrWhiteSpace(title))
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Biztonságos és egyedi fájlnév generálása
                string originalName = Path.GetFileName(file.FileName);
                string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + originalName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var doc = new Document
                {
                    Title = title,
                    IsPublic = isPublic,
                    FilePath = "/uploads/documents/" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName).ToLower(),
                    FileSize = file.Length,
                    UploadedAt = DateTime.Now
                };

                _context.Documents.Add(doc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Documents");
        }

        //Dokumentum törlése (Csak Admin és Editor)
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc != null)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Documents");
        }

    }
}