namespace StoreApp.Data.Concrete
{
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

        // Stok durumu
        public int StockQuantity { get; set; }

        // İndirim oranı (% cinsinden, nullable)
        public decimal? DiscountRate { get; set; }

        // Kampanya mesajı (örneğin, "Son 3 Gün!")
        public string? CampaignMessage { get; set; }

        // Renk seçenekleri (örneğin, "FF0000,00FF00,0000FF" formatında)
        public string? Colors { get; set; }

        // Çoktan çoğa ilişki: Bir ürün birden fazla kategoriye ait olabilir
        public List<Category> Categories { get; set; } = new();
    }
}