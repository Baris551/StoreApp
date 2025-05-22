using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO
{
    public class ProfileDTO
    {
        [Required(ErrorMessage = "User Name is required")]
        [MinLength(3, ErrorMessage = "User Name must be at least 3 characters long")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;

        [MinLength(3, ErrorMessage = "Full Name must be at least 3 characters long")]
        public string? FullName { get; set; }

        [RegularExpression(@"^05\d{2}\s\d{3}\s\d{2}\s\d{2}$", ErrorMessage = "Phone Number must be in the format 05XX XXX XX XX")]
        public string? PhoneNumber { get; set; }

        [MinLength(10, ErrorMessage = "Address must be at least 10 characters long")]
        public string? Address { get; set; }

        public DateTime RegisterDate { get; set; }
    }
}