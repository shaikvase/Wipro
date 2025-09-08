using System.ComponentModel.DataAnnotations;

namespace ECommerceDemo.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = "";

        [Required, StringLength(300)]
        public string Description { get; set; } = "";

        [Range(1, 100000)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = "General";

        [Range(0, 10000)]
        public int Stock { get; set; }

        public string ImageUrl { get; set; } = "/img/placeholder.png";
        public string Slug => Name.ToLower().Replace(' ', '-');
    }
}
