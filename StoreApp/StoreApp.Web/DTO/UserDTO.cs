using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO
{
    public class UserDTO
    {
        public string? FullName { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [MinLength(3, ErrorMessage = "User Name must be at least 3 characters long")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required", AllowEmptyStrings = false)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime RegisterDate { get; set; }

        public string? Address { get; set; }
    }
}