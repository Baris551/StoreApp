using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StoreApp.Web.DTO;

// Ürün oluşturma/güncelleme işlemlerinde kullanılan DTO
public class ProductDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }

    // Seçilen kategori ID'leri (örneğin checkbox ile gelenler)
    [Required(ErrorMessage = "En az bir kategori seçmelisiniz.")]
    public List<int> CategoryIds { get; set; } = new List<int>();

    // Kullanıcıya sunulacak kategori listesi (dropdown veya checkbox için)
    public List<CategoryDTO> AvailableCategories { get; set; } = new List<CategoryDTO>();
    public string? ImageUrl { get; set; }

    // Yüklenen görsel dosyası
    public IFormFile? ImageFile { get; set; }
}

// Kategori bilgilerini taşıyan DTO
public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}




