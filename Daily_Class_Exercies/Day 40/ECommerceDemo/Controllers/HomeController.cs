using ECommerceDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _products;
        public HomeController(ProductService products) => _products = products;

        [HttpGet("")]
        [HttpGet("home")]
        [HttpGet("products")]
        public IActionResult Index()
        {
            return View(_products.GetAll());
        }

        [HttpGet("about")]
        public IActionResult About() => View();

        [HttpGet("contact")]
        public IActionResult Contact() => View();

        public IActionResult Error() => View("~/Views/Shared/Error.cshtml");
    }
}
