using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VBJWeboldal.Models;
using VBJWeboldal.ViewModels;

namespace VBJWeboldal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // --- REGISZTRÁCIÓ ---
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Lekérdezzük, van-e már Admin a rendszerben, mielőtt egyáltalán megmutatjuk az oldalt
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.AdminExists = admins.Any();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. BIZTONSÁGI VÉDELEM: Megnézzük a POST kérés legelején is!
            // Ha valaki megpróbálná kikerülni a HTML formot, itt elkapjuk.
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Any())
            {
                ViewBag.AdminExists = true;
                return View(model); // Azonnal visszadobjuk a View-ra, esélye sincs lefutni a regisztrációnak!
            }

            // 2. HA MÉG NINCS ADMIN, csak akkor engedjük tovább a regisztrációt:
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Automatikus bejelentkezés sikeres regisztráció után
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Admin");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.AdminExists = false; // Ha idáig eljut, akkor biztosan nincs még admin
            return View(model);
        }

        // --- BEJELENTKEZÉS ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            //// 1. reCAPTCHA ELLENŐRZÉS
            //var captchaResponse = Request.Form["g-recaptcha-response"];

            //// Ha a felhasználó be sem pipálta a dobozt
            //if (string.IsNullOrEmpty(captchaResponse))
            //{
            //    ModelState.AddModelError(string.Empty, "Kérjük, igazold, hogy nem vagy robot!");
            //    return View(model);
            //}

            //// Ha bepipálta, leellenőrizzük a Google szerverén, hogy érvényes-e
            //var secretKey = _configuration["ReCaptcha:SecretKey"];
            //using (var client = new HttpClient())
            //{
            //    var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}", null);
            //    var jsonString = await response.Content.ReadAsStringAsync();

            //    if (!jsonString.Contains("\"success\": true"))
            //    {
            //        ModelState.AddModelError(string.Empty, "A robot-ellenőrzés elbukott. Próbáld újra!");
            //        return View(model);
            //    }
            //}

            //AZ EREDETI BEJELENTKEZÉSI LOGIKA
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Admin");
                }
                ModelState.AddModelError(string.Empty, "Sikertelen bejelentkezés. Ellenőrizd az adatokat!");
            }
            return View(model);
        }

        // --- KIJELENTKEZÉS ---
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}