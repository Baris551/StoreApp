using System;
using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO;

public class RegisterDTO
{
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "FullName is required")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    public string? Address { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    
    public string? ReturnUrl { get; set; }

}
