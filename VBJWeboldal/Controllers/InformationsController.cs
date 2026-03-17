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
using Microsoft.Extensions.Caching.Memory;
using System;

namespace VBJWeboldal.Controllers
{
    public class InformationsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache; // <-- Ezt adjuk hozzá!

        public InformationsController(
            IWebHostEnvironment env,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache) // <-- Ezt is be kell kérni!
        {
            _env = env;
            _context = context;
            _userManager = userManager;
            _cache = cache; // <-- És elmenteni!
        }

        // --- KERESÉS FUNKCIÓ ---
        [HttpGet]
        public async Task<IActionResult> Search(string q, bool searchNews, bool searchEvents, bool searchDocuments, bool searchGalleries, bool searchUsers, bool filterSubmitted = false)
        {
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

            if (string.IsNullOrWhiteSpace(q)) return View(vm);

            string lowerQ = q.ToLower();

            if (searchNews) vm.NewsResults = await _context.News.Where(n => n.IsPublished && (n.Title.ToLower().Contains(lowerQ) || n.Content.ToLower().Contains(lowerQ))).OrderByDescending(n => n.PublishedAt).ToListAsync();
            if (searchEvents) vm.EventResults = await _context.Events.Where(e => e.Title.ToLower().Contains(lowerQ) || e.Description.ToLower().Contains(lowerQ)).OrderBy(e => e.EventDate).ToListAsync();
            if (searchDocuments) vm.DocumentResults = await _context.Documents.Where(d => d.IsPublic && d.Title.ToLower().Contains(lowerQ)).OrderByDescending(d => d.UploadedAt).ToListAsync();
            if (searchGalleries) vm.GalleryResults = await _context.Galleries.Where(g => g.Title.ToLower().Contains(lowerQ)).ToListAsync();

            if (searchUsers)
            {
                var editors = await _userManager.GetUsersInRoleAsync("Editor");
                var readers = await _userManager.GetUsersInRoleAsync("Reader");
                var galleryManagers = await _userManager.GetUsersInRoleAsync("GalleryManager");

                var allStaff = editors.Concat(readers).Concat(galleryManagers).GroupBy(u => u.Id).Select(g => g.First()).ToList();
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var adminIds = admins.Select(a => a.Id).ToHashSet();

                vm.UserResults = allStaff.Where(u => !adminIds.Contains(u.Id) && (u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(q, StringComparison.OrdinalIgnoreCase))).OrderBy(u => u.FullName).ToList();
            }

            return View(vm);
        }

        // --- ÓRAREND FUNKCIÓK ---
        public IActionResult Timetable() => View();

        [HttpGet]
        public IActionResult GetTimetableData(string type, string value)
        {
            var data = GetTimetableDataFromFile();
            var result = new TimetableResultViewModel { SelectedValue = value };

            // Biztonsági ellenőrzés: ha a JavaScript null vagy üres értéket küld, adjuk vissza az összes órát
            if (string.IsNullOrEmpty(value))
            {
                result.Lessons = data.Lessons;
                return Json(result);
            }

            if (type == "Class" && data.HomeroomTeachers.ContainsKey(value))
            {
                result.HomeroomTeacher = data.HomeroomTeachers[value];
            }

            // Kis- és nagybetű független keresés (OrdinalIgnoreCase)
            switch (type)
            {
                case "Class": result.Lessons = data.Lessons.Where(l => l.ClassName != null && l.ClassName.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList(); break;
                case "Teacher": result.Lessons = data.Lessons.Where(l => l.Teacher != null && l.Teacher.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList(); break;
                case "Room": result.Lessons = data.Lessons.Where(l => l.Room != null && l.Room.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList(); break;
            }

            return Json(result);
        }

        private (List<LessonViewModel> Lessons, Dictionary<string, string> HomeroomTeachers) GetTimetableDataFromFile()
        {
            var lessons = new List<LessonViewModel>();
            var hrTeachers = new Dictionary<string, string>();

            string xmlPath = Path.Combine(_env.WebRootPath, "uploads", "orarendx.xml");

            if (!System.IO.File.Exists(xmlPath))
            {
                Console.WriteLine($"[HIBA] Nem található az órarend fájl ezen a helyen: {xmlPath}");
                return (lessons, hrTeachers);
            }

            try
            {
                var doc = XDocument.Load(xmlPath);

                var subjects = doc.Descendants("subject").ToDictionary(x => x.Attribute("id")?.Value ?? "", x => x.Attribute("name")?.Value ?? "");
                var teachers = doc.Descendants("teacher").ToDictionary(x => x.Attribute("id")?.Value ?? "", x => x.Attribute("name")?.Value ?? "");
                var classes = doc.Descendants("class").ToDictionary(x => x.Attribute("id")?.Value ?? "", x => x.Attribute("name")?.Value ?? "");
                var classrooms = doc.Descendants("classroom").ToDictionary(x => x.Attribute("id")?.Value ?? "", x => x.Attribute("name")?.Value ?? "");
                var groups = doc.Descendants("group").ToDictionary(x => x.Attribute("id")?.Value ?? "", x => x.Attribute("name")?.Value ?? "");

                foreach (var c in doc.Descendants("class"))
                {
                    var cName = c.Attribute("name")?.Value;
                    var tId = c.Attribute("teacherid")?.Value;
                    if (!string.IsNullOrEmpty(cName) && !string.IsNullOrEmpty(tId) && teachers.ContainsKey(tId))
                    {
                        hrTeachers[cName] = teachers[tId];
                    }
                }

                var lessonsDict = doc.Descendants("lesson").ToDictionary(
                    x => x.Attribute("id")?.Value ?? "",
                    x => new {
                        SubjectId = x.Attribute("subjectid")?.Value,
                        ClassIds = x.Attribute("classids")?.Value,
                        TeacherIds = x.Attribute("teacherids")?.Value,
                        GroupIds = x.Attribute("groupids")?.Value
                    }
                );

                foreach (var card in doc.Descendants("card"))
                {
                    var lessonId = card.Attribute("lessonid")?.Value;
                    if (string.IsNullOrEmpty(lessonId) || !lessonsDict.ContainsKey(lessonId)) continue;

                    var lessonInfo = lessonsDict[lessonId];
                    var periodStr = card.Attribute("period")?.Value;
                    int.TryParse(periodStr, out int period);
                    var daysStr = card.Attribute("days")?.Value;
                    string dayName = GetDayNameFromAsc(daysStr);

                    var roomIds = card.Attribute("classroomids")?.Value ?? "";
                    string roomName = string.Join(", ", roomIds.Split(',').Where(id => classrooms.ContainsKey(id)).Select(id => classrooms[id]));

                    string subjectName = lessonInfo.SubjectId != null && subjects.ContainsKey(lessonInfo.SubjectId) ? subjects[lessonInfo.SubjectId] : "";
                    string teacherName = string.Join(", ", (lessonInfo.TeacherIds ?? "").Split(',').Where(id => teachers.ContainsKey(id)).Select(id => teachers[id]));
                    string className = string.Join(", ", (lessonInfo.ClassIds ?? "").Split(',').Where(id => classes.ContainsKey(id)).Select(id => classes[id]));
                    string groupName = string.Join(", ", (lessonInfo.GroupIds ?? "").Split(',').Where(id => groups.ContainsKey(id)).Select(id => groups[id]));

                    if (string.IsNullOrEmpty(groupName)) groupName = "Teljes osztály";

                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(subjectName))
                    {
                        lessons.Add(new LessonViewModel
                        {
                            Day = dayName,
                            Period = period,
                            ClassName = className,
                            Subject = subjectName,
                            Teacher = teacherName,
                            Room = roomName,
                            Group = groupName
                        });
                    }
                }

                // SIKERES BEOLVASÁS KIÍRÁSA A KONZOLBA!
                Console.WriteLine($"[ÓRAREND OK] aSc XML beolvasva! Talált tanórák száma: {lessons.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[HIBA] aSc XML feldolgozásakor: " + ex.Message);
            }

            return (lessons, hrTeachers);
        }

        private string GetDayNameFromAsc(string days)
        {
            if (string.IsNullOrEmpty(days)) return "Ismeretlen";
            if (days.StartsWith("1")) return "Hétfő";
            if (days.Length > 1 && days[1] == '1') return "Kedd";
            if (days.Length > 2 && days[2] == '1') return "Szerda";
            if (days.Length > 3 && days[3] == '1') return "Csütörtök";
            if (days.Length > 4 && days[4] == '1') return "Péntek";
            if (days.Length > 5 && days[5] == '1') return "Szombat";
            if (days.Length > 6 && days[6] == '1') return "Vasárnap";
            return "Ismeretlen";
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Cafeteria()
        {
            return View();
        }
        // DOKUMENTUMOK LEKÉRDEZÉSE
        [HttpGet("Informations/Documents")]
        public async Task<IActionResult> Documents()
        {
            // Lekérjük az ÖSSZES publikus dokumentumot, a legújabbal kezdve
            var publicDocs = await _context.Documents
                .Where(d => d.IsPublic)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return View(publicDocs);
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
