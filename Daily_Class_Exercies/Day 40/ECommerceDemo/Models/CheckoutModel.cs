using System.ComponentModel.DataAnnotations;

namespace ECommerceDemo.Models
{
    public class CheckoutModel
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(60)]
        public string City { get; set; } = "";

        [Required, StringLength(60)]
        public string State { get; set; } = "";

        [Required, StringLength(60)]
        public string Country { get; set; } = "";
    }
}
