using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Xml.Linq;
using VBJWeboldal.ViewModels;

namespace VBJWeboldal.Controllers
{
    public class InformationsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public InformationsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Timetable() => View();

        [HttpGet]
        public IActionResult GetTimetableOptions(string type)
        {
            string path = Path.Combine(_env.WebRootPath, "uploads", "timetable.xml");
            if (!System.IO.File.Exists(path)) return Json(new List<string>());

            var doc = XDocument.Load(path);
            var options = doc.Descendants("Lesson")
                             .Select(l => l.Attribute(type)?.Value)
                             .Distinct().OrderBy(x => x);
            return Json(options);
        }

        [HttpGet]
        public IActionResult GetTimetableData(string type, string value)
        {
            string path = Path.Combine(_env.WebRootPath, "uploads", "timetable.xml");
            if (!System.IO.File.Exists(path)) return NotFound();

            var doc = XDocument.Load(path);
            var res = new TimetableResultViewModel { SelectedValue = value };

            if (type == "Class")
            {
                res.HomeroomTeacher = doc.Descendants("Class")
                    .FirstOrDefault(c => c.Attribute("Name")?.Value == value)?.Attribute("Teacher")?.Value;
            }

            res.Lessons = doc.Descendants("Lesson")
                .Where(l => l.Attribute(type)?.Value == value)
                .Select(l => new LessonViewModel
                {
                    Day = l.Attribute("Day")?.Value,
                    Period = int.Parse(l.Attribute("Period")?.Value ?? "0"),
                    ClassName = l.Attribute("Class")?.Value,
                    Subject = l.Attribute("Subject")?.Value,
                    Teacher = l.Attribute("Teacher")?.Value,
                    Room = l.Attribute("Room")?.Value
                }).ToList();

            return Json(res);
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
