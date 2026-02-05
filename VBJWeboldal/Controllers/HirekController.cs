using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    public class HirekController : Controller
    {
        [Route("/hirek")]
        public IActionResult News()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
