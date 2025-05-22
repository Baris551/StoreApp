namespace StoreApp.Data.Concrete
{
    // Ürün modelini temsil eder
    public class Product
    {
        public int Id { get; set; }

        // Ürün adı (örneğin: iPhone 14)
        public string Name { get; set; }

        // Ürün fiyatı
        public decimal Price { get; set; }

        // Ürün açıklaması
        public string Description { get; set; }

        // Ürün görseli için dosya yolu (örnek: "/images/products/urun1.jpg")
        public string? ImageUrl { get; set; }

        // Çoktan çoğa ilişki: Bir ürün birden fazla kategoriye ait olabilir
        public List<Category> Categories { get; set; } = new();
    }
}
