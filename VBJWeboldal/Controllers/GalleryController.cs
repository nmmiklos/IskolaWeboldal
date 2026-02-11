using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    [Route("galeria")]
    public class GalleryController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
