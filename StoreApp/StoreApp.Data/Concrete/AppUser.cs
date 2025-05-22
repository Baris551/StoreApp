using System;
using Microsoft.AspNetCore.Identity;

namespace StoreApp.Data.Concrete;

public class AppUser : IdentityUser<int>
{
    public string? FullName { get; set; }
    public DateTime DateAdded { get; set; }
    public string? Address { get; set; }
    public string PhoneNumber { get; set; }
}
