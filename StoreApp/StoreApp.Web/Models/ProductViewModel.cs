namespace StoreApp.Web.Models;

// Ürünleri temsil eden view modeli.
// Bu sınıf, kullanıcıya gösterilecek ürün bilgilerini taşır.
public class ProductViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } // Fotoğraf dosya yolu
}
// Birden fazla ürünün listelendiği view için kullanılan model.
// Genellikle ürünlerin listelendiği sayfalarda kullanılır.
public class ProductListViewModel
{
    // Ürünlerin listesi (boş başlatılır ki hata vermesin)
    public IEnumerable<ProductViewModel> Products { get; set; } = Enumerable.Empty<ProductViewModel>();
    public PageInfo PageInfo { get; set; } = new (); // Sayfa bilgilerini tutar. Toplam ürün sayısı, sayfa sayısı gibi bilgiler burada tutulur.
}
public class PageInfo
{
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    public int CurrentPage { get; set; }
}

















//  Peki neden 2 ayrı class var?
// ProductViewModel:
// Tek bir ürünü temsil eder. Mesela bir ürün detay sayfasında kullanılabilir.
// ProductListViewModel:
// Birden çok ürünü taşıyan bir listedir. Genelde ürünleri listelediğin bir sayfada (Index.cshtml gibi) kullanılır.
// Bu yapı sayesinde:
// Kod daha düzenli olur.
// Hangi view hangi veriyi kullanıyor daha net olur.
// Her sayfa sadece ihtiyacı olan veriyi alır, gereksiz data taşımaz

