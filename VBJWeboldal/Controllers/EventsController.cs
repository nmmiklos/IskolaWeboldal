using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    [Route("esemenyek")]
    public class EventsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
