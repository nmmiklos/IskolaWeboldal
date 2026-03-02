using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VBJWeboldal.Data;
using VBJWeboldal.Models;
using VBJWeboldal.ViewModels;
using ClosedXML.Excel;

namespace VBJWeboldal.Controllers
{
    public class InformationsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Injektáljuk az Adatbázist és a UserManagert is!
        public InformationsController(
            IWebHostEnvironment env,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _env = env;
            _context = context;
            _userManager = userManager;
        }

        // --- KERESÉS FUNKCIÓ (Frissített) ---
        [HttpGet]
        public async Task<IActionResult> Search(string q, bool searchNews, bool searchEvents, bool searchDocuments, bool searchGalleries, bool searchUsers, bool filterSubmitted = false)
        {
            // Ha a főoldalról jön (nincs szűrő elküldve), alapértelmezetten mindenben keressen!
            if (!filterSubmitted)
            {
                searchNews = searchEvents = searchDocuments = searchGalleries = searchUsers = true;
            }

            var vm = new SearchResultsViewModel
            {
                SearchQuery = q ?? "",
                SearchNews = searchNews,
                SearchEvents = searchEvents,
                SearchDocuments = searchDocuments,
                SearchGalleries = searchGalleries,
                SearchUsers = searchUsers
            };

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(vm);
            }

            string lowerQ = q.ToLower();

            // 1. Hírek
            if (searchNews)
            {
                vm.NewsResults = await _context.News.Where(n => n.IsPublished && (n.Title.ToLower().Contains(lowerQ) || n.Content.ToLower().Contains(lowerQ))).OrderByDescending(n => n.PublishedAt).ToListAsync();
            }

            // 2. Események
            if (searchEvents)
            {
                vm.EventResults = await _context.Events.Where(e => e.Title.ToLower().Contains(lowerQ) || e.Description.ToLower().Contains(lowerQ)).OrderBy(e => e.EventDate).ToListAsync();
            }

            // 3. Dokumentumok
            if (searchDocuments)
            {
                vm.DocumentResults = await _context.Documents.Where(d => d.IsPublic && d.Title.ToLower().Contains(lowerQ)).OrderByDescending(d => d.UploadedAt).ToListAsync();
            }

            // 4. Galéria
            if (searchGalleries)
            {
                vm.GalleryResults = await _context.Galleries.Where(g => g.Title.ToLower().Contains(lowerQ)).ToListAsync();
            }

            // 5. Kapcsolatok (Tanárok)
            if (searchUsers)
            {
                var editors = await _userManager.GetUsersInRoleAsync("Editor");
                var readers = await _userManager.GetUsersInRoleAsync("Reader");
                var galleryManagers = await _userManager.GetUsersInRoleAsync("GalleryManager");

                var allStaff = editors.Concat(readers).Concat(galleryManagers).GroupBy(u => u.Id).Select(g => g.First()).ToList();
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var adminIds = admins.Select(a => a.Id).ToHashSet();

                vm.UserResults = allStaff.Where(u => !adminIds.Contains(u.Id) && (u.FullName.Contains(q, System.StringComparison.OrdinalIgnoreCase) || u.Email.Contains(q, System.StringComparison.OrdinalIgnoreCase))).OrderBy(u => u.FullName).ToList();
            }

            return View(vm);
        }

        public IActionResult Timetable() => View();

        // -------------------------------------------------------------
        // SEGÉDFÜGGVÉNY: Beolvassa a fájlt (XML vagy XLSX), és egy közös listát ad vissza
        // -------------------------------------------------------------
        private (List<LessonViewModel> Lessons, Dictionary<string, string> HomeroomTeachers) GetTimetableDataFromFile()
        {
            var lessons = new List<LessonViewModel>();
            var hrTeachers = new Dictionary<string, string>();

            string xmlPath = Path.Combine(_env.WebRootPath, "uploads", "timetable.xml");
            string xlsxPath = Path.Combine(_env.WebRootPath, "uploads", "timetable.xlsx");

            // --- EXCEL FELDOLGOZÁS ---
            if (System.IO.File.Exists(xlsxPath))
            {
                using (var workbook = new XLWorkbook(xlsxPath))
                {
                    // 1. Osztályfőnökök beolvasása (HomeroomTeachers munkalap)
                    if (workbook.TryGetWorksheet("HomeroomTeachers", out var hrSheet))
                    {
                        var rows = hrSheet.RowsUsed().Skip(1); // Fejléc átugrása
                        foreach (var row in rows)
                        {
                            var className = row.Cell(1).GetString();
                            var teacherName = row.Cell(2).GetString();
                            if (!string.IsNullOrEmpty(className)) hrTeachers[className] = teacherName;
                        }
                    }

                    // 2. Órák beolvasása (Lessons munkalap)
                    if (workbook.TryGetWorksheet("Lessons", out var lessonSheet))
                    {
                        var rows = lessonSheet.RowsUsed().Skip(1); // Fejléc átugrása
                        foreach (var row in rows)
                        {
                            lessons.Add(new LessonViewModel
                            {
                                Day = row.Cell(1).GetString(),
                                Period = row.Cell(2).GetValue<int>(),
                                ClassName = row.Cell(3).GetString(),
                                Subject = row.Cell(4).GetString(),
                                Teacher = row.Cell(5).GetString(),
                                Room = row.Cell(6).GetString(),
                                Group = row.Cell(7).GetString()
                            });
                        }
                    }
                }
            }
            // --- XML FELDOLGOZÁS (Visszafelé kompatibilitás) ---
            else if (System.IO.File.Exists(xmlPath))
            {
                var doc = XDocument.Load(xmlPath);

                foreach (var ht in doc.Descendants("Class"))
                {
                    hrTeachers[ht.Attribute("Name")?.Value ?? ""] = ht.Attribute("Teacher")?.Value ?? "";
                }

                foreach (var l in doc.Descendants("Lesson"))
                {
                    lessons.Add(new LessonViewModel
                    {
                        Day = l.Attribute("Day")?.Value,
                        Period = int.Parse(l.Attribute("Period")?.Value ?? "0"),
                        ClassName = l.Attribute("Class")?.Value,
                        Subject = l.Attribute("Subject")?.Value,
                        Teacher = l.Attribute("Teacher")?.Value,
                        Room = l.Attribute("Room")?.Value,
                        Group = l.Attribute("Group")?.Value
                    });
                }
            }

            return (lessons, hrTeachers);
        }

        // -------------------------------------------------------------
        // AJAX: Opciók lekérése
        // -------------------------------------------------------------
        [HttpGet]
        public IActionResult GetTimetableOptions(string type)
        {
            var data = GetTimetableDataFromFile();
            var options = new List<string>();

            switch (type)
            {
                case "Class": options = data.Lessons.Select(l => l.ClassName).Distinct().ToList(); break;
                case "Teacher": options = data.Lessons.Select(l => l.Teacher).Distinct().ToList(); break;
                case "Room": options = data.Lessons.Select(l => l.Room).Distinct().ToList(); break;
            }

            return Json(options.Where(o => !string.IsNullOrEmpty(o)).OrderBy(x => x));
        }

        // -------------------------------------------------------------
        // AJAX: Órarend adatok lekérése
        // -------------------------------------------------------------
        [HttpGet]
        public IActionResult GetTimetableData(string type, string value)
        {
            var data = GetTimetableDataFromFile();
            var result = new TimetableResultViewModel { SelectedValue = value };

            if (type == "Class" && data.HomeroomTeachers.ContainsKey(value))
            {
                result.HomeroomTeacher = data.HomeroomTeachers[value];
            }

            switch (type)
            {
                case "Class": result.Lessons = data.Lessons.Where(l => l.ClassName == value).ToList(); break;
                case "Teacher": result.Lessons = data.Lessons.Where(l => l.Teacher == value).ToList(); break;
                case "Room": result.Lessons = data.Lessons.Where(l => l.Room == value).ToList(); break;
            }

            return Json(result);
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Cafeteria()
        {
            return View();
        }
        public IActionResult Documents()
        {
            return View();
        }
        public IActionResult Education()
        {
            return View();
        }
        public IActionResult Spiritual()
        {
            return View();
        }
    }
}
