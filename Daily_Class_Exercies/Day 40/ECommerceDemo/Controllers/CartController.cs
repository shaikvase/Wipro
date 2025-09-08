using ECommerceDemo.Filters;
using ECommerceDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceDemo.Controllers
{
    [RequireLogin]
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly CartService _cart;
        private readonly ProductService _products;

        public CartController(CartService cart, ProductService products)
        {
            _cart = cart; _products = products;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Total = _cart.Total();
            return View(_cart.All());
        }

        [HttpPost("{id:int}")]
        public IActionResult Add(int id)
        {
            var p = _products.GetById(id);
            if (p != null) _cart.Add(p);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("{id:int}")]
        public IActionResult Increment(int id)
        {
            _cart.Increment(id);
            return RedirectToAction("Index");
        }

        [HttpPost("{id:int}")]
        public IActionResult Decrement(int id)
        {
            _cart.Decrement(id);
            return RedirectToAction("Index");
        }

        [HttpPost("{id:int}")]
        public IActionResult Remove(int id)
        {
            _cart.Remove(id);
            return RedirectToAction("Index");
        }
    }
}
