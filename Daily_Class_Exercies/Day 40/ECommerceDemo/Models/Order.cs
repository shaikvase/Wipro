namespace ECommerceDemo.Models
{
    public class Order
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        // Customer snapshot
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string Country { get; set; } = "";
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    }
}
