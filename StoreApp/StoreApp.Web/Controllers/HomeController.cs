using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Abstract;
using StoreApp.Web.Models;

namespace StoreApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly IStoreRepository _storeRepository;
    private readonly IMapper _mapper;
    private readonly int _featuredProductsCount = 10; // Öne çıkan ürün sayısı
    public HomeController(IStoreRepository storeRepository, IMapper mapper)
    {
        _storeRepository = storeRepository;
        _mapper = mapper;
    }
    public IActionResult Index()
    {
        // Öne çıkan ürünleri çek (örneğin, rastgele 5 ürün)
        var featuredProducts = _storeRepository.Products
            .OrderBy(p => Guid.NewGuid()) // Rastgele sıralama
            .Take(_featuredProductsCount)
            .Select(product => _mapper.Map<ProductViewModel>(product))
            .ToList();

        return View(featuredProducts);
    }


    // GET: /Home/Error?statusCode={statusCode}
    // Hata sayfasını gösterir (örneğin 404 için)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Önbelleğe alma
    public IActionResult Error(int? statusCode = null)
    {
        // Hata mesajını ve durum kodunu bir model olarak oluşturuyoruz
        var errorModel = new ErrorViewModel
        {
            StatusCode = statusCode ?? 500, // Eğer statusCode yoksa, varsayılan olarak 500 (genel hata)
            Message = statusCode switch
            {
                404 => "Sayfa Bulunamadı", // 404 için özel mesaj
                500 => "Sunucu Hatası", // 500 için özel mesaj
                _ => "Bir Hata Oluştu" // Diğer durumlar için genel mesaj
            }
        };

        // Durum kodunu HTTP yanıtına ayarlıyoruz
        Response.StatusCode = errorModel.StatusCode;

        // Error.cshtml sayfasını döndürüyoruz ve modeli gönderiyoruz
        return View(errorModel);

    }
}


// public int pageSize = 3; // Number of products per page
//     private IStoreRepository _storeRepository;
//     public HomeController(IStoreRepository storeRepository)
//     {
//         _storeRepository = storeRepository;
//     }

//     localhost:5000/?page=1
//     localhost:5000/products/2
//     localhost:5000/products/page/2
//     public IActionResult Index(int page = 1) 
//     {
//         var products = _storeRepository
//             .Products
//             .Skip((page - 1) * pageSize)   
//             .Select(p => 
//                 new ProductViewModel {
//                     Id = p.Id,
//                     Name = p.Name,
//                     Description = p.Description,
//                     Price = p.Price
//                 }).Take(pageSize);

//         return View(new ProductListViewModel {
//             Products = products,
//             pageInfo = new PageInfo {
//                 ItemsPerPage = pageSize,
//                 CurrentPage = page,
//                 TotalItems = _storeRepository.Products.Count()
                
//             }
//         });

//     }
