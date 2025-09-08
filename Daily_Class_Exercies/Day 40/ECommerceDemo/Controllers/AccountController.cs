using ECommerceDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceDemo.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new UserLogin());
        }

        [HttpPost]
        public IActionResult Login(UserLogin model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            // For demo: any email/password passes (after validation)
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("UserEmail", model.Email);
            return Redirect(string.IsNullOrEmpty(returnUrl) ? Url.Action("Index", "Home")! : returnUrl);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
