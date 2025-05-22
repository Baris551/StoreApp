using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Eski şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre onayı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}