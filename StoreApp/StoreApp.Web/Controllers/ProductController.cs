using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Abstract;
using StoreApp.Web.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace StoreApp.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly int _pageSize = 12;
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public ProductController(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }


        // Ürünleri listeleyen ve sayfalayan metod (asenkron)
        public async Task<IActionResult> Index(string category, string sort, int page = 1)
        {
            // Eğer kategori belirtilmemişse veya boşsa, null yap bu sayede tüm kategorilerdeki ürünler gösterilir
            if (string.IsNullOrEmpty(category))
            {
                category = null;
            }
            // Görünümde bu bilgi, örneğin kategori seçimi için kullanılabilir - Viewbag ile viewe gönder
            ViewBag.SelectedCategory = category;

            // IStoreRepository üzerinden ürünleri kategoriye, sayfaya ve sıralamaya göre getir
            var productsQuery = await _storeRepository.GetProductsByCategoryAsync(category, page, _pageSize, sort);

            // Ürünleri ProductViewModel'e dönüştür
            // AutoMapper, Product nesnesini ProductViewModel nesnesine çevirir
            var products = await productsQuery
                .Select(product => _mapper.Map<ProductViewModel>(product))
                .ToListAsync();

            // Her ürün için görsel kontrolü yap
            foreach (var product in products)
            {
                // Eğer ürünün görsel URL'si yoksa, varsayılan bir görsel ata
                if (string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = "/images/default-product.jpg"; // Varsayılan görsel yolu
                }
            }

            // Görünüme gönderilecek modeli hazırla
            return View(new ProductListViewModel
            {
                Products = products, // Ürün listesi
                PageInfo = new PageInfo
                {
                    ItemsPerPage = _pageSize, // Her sayfada gösterilecek ürün sayısı
                    CurrentPage = page, // Şu anki sayfa numarası
                    TotalItems = await _storeRepository.GetProductCountAsync(category) // Toplam ürün sayısı (asenkron)
                }
            });
        }

        // Belirtilen ID'ye sahip ürünün detaylarını gösteren metod (asenkron)
        public async Task<IActionResult> Details(int id)
        {
            // Ürünü ID'sine göre veritabanından getir
            var product = await _storeRepository.GetProductByIdAsync(id);

            // Eğer ürün bulunamazsa, 404 (Not Found) hatası döndür
            if (product == null)
            {
                return NotFound();
            }

            // Ürünü ProductViewModel'e dönüştür
            var viewModel = _mapper.Map<ProductViewModel>(product);
            // Ürün renklerini virgülle ayrılmış string'den bir listeye çevir
            // Eğer renkler null ise, boş bir liste döndür
            viewModel.Colors = product.Colors?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            // Stok kontrolü yap
            if (viewModel.StockQuantity <= 0)
            {
                // Eğer stok yoksa, hata mesajını TempData ile görünüme gönder
                TempData["ErrorMessage"] = "Bu ürün stokta bulunmamaktadır.";
            }

            // Ürün detaylarını görünüme (view) gönder
            return View(viewModel);
        }

    }
}