using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Data.Concrete;
using StoreApp.Web.DTO;
using System.IO;
using System.Threading.Tasks;

namespace StoreApp.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(StoreDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Categories)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    CategoryIds = p.Categories.Select(c => c.Id).ToList(),
                    AvailableCategories = _context.Categories
                        .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                        .ToList()
                })
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var model = new ProductDTO
            {
                AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductDTO model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    Categories = new List<Category>()
                };

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = $"{DateTime.Now.Ticks}_{model.ImageFile.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/images/products/{uniqueFileName}";
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Product saved with ID: {product.Id}");

                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    Console.WriteLine($"Selected Category IDs: {string.Join(", ", model.CategoryIds)}");
                    var selectedCategories = await _context.Categories
                        .Where(c => model.CategoryIds.Contains(c.Id))
                        .ToListAsync();
                    Console.WriteLine($"Found Categories: {selectedCategories.Count}");
                    if (selectedCategories.Any())
                    {
                        Console.WriteLine($"Categories to be added: {string.Join(", ", selectedCategories.Select(c => c.Name))}");
                        foreach (var category in selectedCategories)
                        {
                            var productCategory = new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = category.Id
                            };
                            _context.Set<ProductCategory>().Add(productCategory);
                            Console.WriteLine($"Adding ProductCategory: ProductId={productCategory.ProductId}, CategoryId={productCategory.CategoryId}");
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    Console.WriteLine("No categories selected.");
                }

                var addedProduct = await _context.Products
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
                Console.WriteLine($"Added Product ID: {addedProduct.Id}, Categories: {string.Join(", ", addedProduct.Categories.Select(c => c.Name))}");

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryIds = product.Categories.Select(c => c.Id).ToList(),
                AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductDTO model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                product.Name = model.Name;
                product.Price = model.Price; // Fiyat burada doğru şekilde decimal olarak alınıyor
                product.Description = model.Description;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = $"{DateTime.Now.Ticks}_{model.ImageFile.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/images/products/{uniqueFileName}";
                }

                var existingCategories = _context.Set<ProductCategory>()
                    .Where(pc => pc.ProductId == product.Id);
                _context.Set<ProductCategory>().RemoveRange(existingCategories);

                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    var selectedCategories = await _context.Categories
                        .Where(c => model.CategoryIds.Contains(c.Id))
                        .ToListAsync();

                    foreach (var category in selectedCategories)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = category.Id
                        };
                        _context.Set<ProductCategory>().Add(productCategory);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productCategories = _context.Set<ProductCategory>()
                .Where(pc => pc.ProductId == id);
            _context.Set<ProductCategory>().RemoveRange(productCategories);

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}