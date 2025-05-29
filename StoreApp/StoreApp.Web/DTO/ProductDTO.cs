using System.ComponentModel.DataAnnotations;

namespace StoreApp.Web.DTO
{
    public class ProductDTO
    {
        // Ürünün veritabanındaki kimlik numarası (ID). Her ürün için benzersiz bir numara.
        public int Id { get; set; }

        // Ürünün adı. Zorunlu bir alan, kullanıcı mutlaka bir isim girmeli.
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        public string Name { get; set; }

        // Ürünün fiyatı. Zorunlu bir alan ve 0’dan büyük olmalı.
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat sıfırdan büyük olmalıdır.")]
        public decimal Price { get; set; }

        // Ürünün açıklaması. Bu alan isteğe bağlı, kullanıcı isterse doldurur.
        public string Description { get; set; }

        // Ürünün resminin yolunu tutar (örneğin, "/images/products/123.jpg"). İsteğe bağlı bir alan.
        // [Required] attribute’ü yok, çünkü zorunlu değil.
        public string ImageUrl { get; set; }

        // Kullanıcının yüklediği resim dosyası. İsteğe bağlı, yani kullanıcı resim yüklemese de olur.
        // [Required] attribute’ü yok, çünkü zorunlu değil.
        public IFormFile ImageFile { get; set; }

        // Ürünün stok miktarı. Zorunlu bir alan ve negatif olamaz.
        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz.")]
        public int StockQuantity { get; set; }

        // Ürünün indirim oranı (%). İsteğe bağlı bir alan, 0 ile 100 arasında olabilir.
        [Range(0, 100, ErrorMessage = "İndirim oranı 0 ile 100 arasında olmalıdır.")]
        public decimal? DiscountRate { get; set; }

        // Ürünün kampanya mesajı (örneğin, "Son 3 Gün!"). İsteğe bağlı bir alan.
        public string CampaignMessage { get; set; }

        // Ürünün renk seçenekleri (örneğin, "FF0000,00FF00"). İsteğe bağlı bir alan.
        public string Colors { get; set; }

        // Ürünün seçilen kategorilerinin ID’leri. Örneğin, [1, 2, 3] gibi bir liste.
        public List<int> CategoryIds { get; set; } = new List<int>();

        // Kullanıcının seçebileceği tüm kategoriler. Formda kategori seçimi için kullanılır.
        public List<CategoryDTO> AvailableCategories { get; set; } = new List<CategoryDTO>();
    }

    // Kategori bilgilerini tutmak için bir yardımcı sınıf.
    public class CategoryDTO
    {
        // Kategorinin kimlik numarası (ID).
        public int Id { get; set; }

        // Kategorinin adı (örneğin, "Elektronik").
        public string Name { get; set; }
    }
}