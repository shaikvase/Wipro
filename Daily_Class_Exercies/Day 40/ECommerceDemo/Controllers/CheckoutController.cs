using ECommerceDemo.Filters;
using ECommerceDemo.Models;
using ECommerceDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceDemo.Controllers
{
    [RequireLogin]
    [Route("[controller]/[action]")]
    public class CheckoutController : Controller
    {
        private readonly CartService _cart;

        public CheckoutController(CartService cart) => _cart = cart;

        [HttpGet]
        public IActionResult Index()
        {
            if (!_cart.All().Any()) return RedirectToAction("Index", "Cart");
            return View(new CheckoutModel());
        }

        [HttpPost]
        public IActionResult Index(CheckoutModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var order = new Order
            {
                Name = model.Name,
                Email = model.Email,
                City = model.City,
                State = model.State,
                Country = model.Country,
                Items = _cart.All()
            };
            // Clear cart after snapshot
            _cart.Clear();

            TempData["OrderId"] = order.OrderId.ToString();
            return View("OrderSummary", order);
        }
    }
}
