using ECommerceDemo.Models;

namespace ECommerceDemo.Services
{
    public class ProductService
    {
        private readonly List<Product> _products = new()
        {
            new Product { Id=1, Name="Wireless Headphones", Description="Noise cancelling over-ear", Price=4999, Category="Electronics", Stock=20, ImageUrl="https://picsum.photos/seed/1/600/400" },
            new Product { Id=2, Name="Smart Watch", Description="Fitness tracking & notifications", Price=7999, Category="Electronics", Stock=15, ImageUrl="https://picsum.photos/seed/2/600/400" },
            new Product { Id=3, Name="Gaming Mouse", Description="Ergonomic, RGB, 7 buttons", Price=1999, Category="Computer", Stock=35, ImageUrl="https://picsum.photos/seed/3/600/400" },
            new Product { Id=4, Name="Backpack", Description="Water-resistant 25L", Price=1499, Category="Fashion", Stock=50, ImageUrl="https://picsum.photos/seed/4/600/400" },
            new Product { Id=5, Name="Bluetooth Speaker", Description="Portable, deep bass", Price=3499, Category="Electronics", Stock=25, ImageUrl="https://picsum.photos/seed/5/600/400" },
            new Product { Id=6, Name="Mechanical Keyboard", Description="Hot-swappable, tactile", Price=3999, Category="Computer", Stock=18, ImageUrl="https://picsum.photos/seed/6/600/400" },
        };

        public IEnumerable<Product> GetAll() => _products;
        public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
    }
}
