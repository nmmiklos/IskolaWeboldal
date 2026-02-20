using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    public class InformationsController : Controller
    {
        public IActionResult Timetable()
        {
            return View();
        }
    }
}
