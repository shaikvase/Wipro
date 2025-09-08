using ECommerceDemo.Models;
using ECommerceDemo.Utils;

namespace ECommerceDemo.Services
{
    public class CartService
    {
        private const string CartKey = "CART";
        private readonly IHttpContextAccessor _http;

        public CartService(IHttpContextAccessor http) => _http = http;

        private List<CartItem> GetCart()
        {
            var session = _http.HttpContext!.Session;
            return session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> items)
        {
            _http.HttpContext!.Session.SetObject(CartKey, items);
        }

        public List<CartItem> All() => GetCart();

        public void Add(Product p, int qty = 1)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == p.Id);
            if (existing == null)
            {
                cart.Add(new CartItem { ProductId = p.Id, Name = p.Name, Price = p.Price, Quantity = Math.Max(1, qty), ImageUrl = p.ImageUrl });
            }
            else
            {
                existing.Quantity += qty;
            }
            SaveCart(cart);
        }

        public void Increment(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null) item.Quantity++;
            SaveCart(cart);
        }

        public void Decrement(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0) cart.Remove(item);
            }
            SaveCart(cart);
        }

        public void Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        public void Clear()
        {
            SaveCart(new List<CartItem>());
        }

        public decimal Total() => GetCart().Sum(i => i.Price * i.Quantity);
        public int Count() => GetCart().Sum(i => i.Quantity);
    }
}
