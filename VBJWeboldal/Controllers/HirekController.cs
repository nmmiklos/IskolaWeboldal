using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    [Route("hirek")]
    public class HirekController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
