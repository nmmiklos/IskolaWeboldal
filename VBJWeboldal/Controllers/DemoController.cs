using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VBJWeboldal.Controllers
{
    public class DemoController : Controller
    {
        // Ebben tároljuk a kommenteket a memóriában. 
        // A string kulcs a felhasználó egyedi azonosítója (Cookie), a List pedig az ő kommentjei.
        private static readonly ConcurrentDictionary<string, List<string>> _isolatedComments = new();

        [HttpGet]
        [Route("/xss-demo")]
        public IActionResult Index()
        {
            // 1. Megnézzük, van-e már egyedi XSS azonosítója a hallgatónak
            string userId = Request.Cookies["XssDemoId"];

            if (string.IsNullOrEmpty(userId))
            {
                // Ha nincs, adunk neki egyet, és elmentjük a böngészőjébe 1 napra
                userId = Guid.NewGuid().ToString();
                Response.Cookies.Append("XssDemoId", userId, new CookieOptions { Expires = DateTime.Now.AddDays(1) });
            }

            // 2. Lekérjük az Ő saját kommentjeit (ha még nincs, adunk egy üres listát)
            var userComments = _isolatedComments.GetOrAdd(userId, new List<string> {
                "<i>Rendszer: Üdv a tesztkörnyezetben! Próbálj meg HTML vagy JS kódot beküldeni!</i>"
            });

            return View(userComments);
        }

        [HttpPost]
        [Route("/xss-demo/add")]
        public IActionResult AddComment(string rawComment)
        {
            string userId = Request.Cookies["XssDemoId"];

            // Ha valamiért nincs azonosító (pl. tiltott sütik), visszadobjuk
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(rawComment))
                return RedirectToAction("Index");

            // SZÁNDÉKOS BIZTONSÁGI HIBA: 
            // Semmilyen validációt vagy HTML Encode-ot nem csinálunk!
            var userComments = _isolatedComments.GetOrAdd(userId, new List<string>());
            userComments.Add(rawComment);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/xss-demo/reset")]
        public IActionResult ResetSandbox()
        {
            // Ezzel a hallgató bármikor kiürítheti a saját falát, ha túl jól sikerült a hackelés
            string userId = Request.Cookies["XssDemoId"];
            if (!string.IsNullOrEmpty(userId))
            {
                _isolatedComments.TryRemove(userId, out _);
            }
            return RedirectToAction("Index");
        }
    }
}