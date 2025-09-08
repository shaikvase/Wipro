using System.ComponentModel.DataAnnotations;

namespace ECommerceDemo.Models
{
    public class UserLogin : IValidatableObject
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = "";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Email.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Use a non-example email.", new[] { nameof(Email) });
            }
        }
    }
}
