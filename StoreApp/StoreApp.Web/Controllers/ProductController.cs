using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Abstract;
using StoreApp.Web.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace StoreApp.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly int _pageSize = 12; // 4 sütun x 3 satır için 12 ürün
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public ProductController(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        // localhost:5000/?page=1
        // localhost:5000/products/2
        // localhost:5000/products/page/2

        public IActionResult Index(string category, int page = 1)
        {
            if(string.IsNullOrEmpty(category))
            {
                category = null;
            }
            ViewBag.SelectedCategory = category;

            var products = _storeRepository.GetProductsByCategory(category, page, _pageSize)
                .Select(product => _mapper.Map<ProductViewModel>(product))
                .ToList();

            return View(new ProductListViewModel
            {
                Products = products,
                PageInfo = new PageInfo
                {
                    ItemsPerPage = _pageSize,
                    CurrentPage = page,
                    TotalItems = _storeRepository.GetProductCount(category)
                }
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _storeRepository.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<ProductViewModel>(product);
            return View(viewModel);
        }    
    }
}