using System.Collections.Generic;

namespace StoreApp.Data.Concrete
{
    public class Category
    {   
        public int Id { get; set; }

        // Kategori adı (örneğin: Telefon, Beyaz Eşya)
        public string? Name { get; set; }

        // URL'de gösterilecek kategori ismi (örneğin: telefon, beyaz-esya)
        public string? Url { get; set; }

        // Kategoriye ait ürünlerin listesi (çoktan çoğa ilişki)
        public List<Product> Products { get; set; } = new();
    }
}
