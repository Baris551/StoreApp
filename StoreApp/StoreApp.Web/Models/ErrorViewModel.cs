namespace StoreApp.Web.Models
{
    // Hata sayfasında kullanılacak model
    public class ErrorViewModel
    {
        // HTTP durum kodu (örneğin 404, 500)
        public int StatusCode { get; set; }

        // Hata mesajı (örneğin "Sayfa Bulunamadı")
        public string Message { get; set; } = null!;
    }
}