using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    public class EventsController : Controller
    {

        [Route("/esemenyek")]
        public IActionResult Events()
        {
            return View();
        }
    }
}
