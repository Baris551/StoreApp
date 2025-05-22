using System;
using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO;

public class LoginDTO
{    
    // Kullanıcı adı alanı, zorunlu ve hata mesajı içeriyor
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = null!;
    
    // Şifre alanı, zorunlu ve hata mesajı içeriyor
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
    
    // ReturnUrl alanı, giriş yaptıktan sonra yönlendirilecek URL’yi tutar
    // Bu alan zorunlu değil (opsiyonel), bu yüzden string? kullanıyoruz
    public string? ReturnUrl { get; set; }
}