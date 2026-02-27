using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using VBJWeboldal.ViewModels;
using ClosedXML.Excel; // EZT ADD HOZZÁ!

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
