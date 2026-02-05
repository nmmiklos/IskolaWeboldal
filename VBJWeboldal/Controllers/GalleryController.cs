using Microsoft.AspNetCore.Mvc;

namespace VBJWeboldal.Controllers
{
    public class GalleryController : Controller
    {
        [Route("/galeria")]
        public IActionResult Gallery()
        {
            return View();
        }
    }
}
