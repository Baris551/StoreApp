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

        // GET: /Admin/Index
        // Bu metod, tüm ürünleri listeler ve admin panelinde gösterir.
        public async Task<IActionResult> Index()
        {
            // Veritabanından tüm ürünleri çekiyoruz.
            // Include(p => p.Categories) ile ürünlerin kategorilerini de getiriyoruz.
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
                    // Tüm kategorileri çekip formda kullanılabilir hale getiriyoruz.
                    AvailableCategories = _context.Categories
                        .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                        .ToList(),
                    // Yeni eklenen alanlar
                    StockQuantity = p.StockQuantity,
                    DiscountRate = p.DiscountRate,
                    CampaignMessage = p.CampaignMessage,
                    Colors = p.Colors
                })
                .ToListAsync();

            // Ürünleri listeleyen sayfayı (Index.cshtml) açıyoruz.
            return View(products);
        }

        // GET: /Admin/AddProduct
        // Bu metod, yeni ürün ekleme formunu açar. Kullanıcıya boş bir form gösterir.
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            // Boş bir ProductDTO oluşturuyoruz.
            var model = new ProductDTO
            {
                // Kategorileri veritabanından çekip formda gösteriyoruz.
                AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync()
            };
            // Yeni ürün ekleme formunu (AddProduct.cshtml) açıyoruz.
            return View(model);
        }

        // POST: /Admin/AddProduct
        // Bu metod, kullanıcının yeni ürün ekleme formunu doldurup gönderdiğinde çalışır. Yeni ürünü kaydeder.
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductDTO model)
        {
            // Eğer ModelState’te ImageUrl veya ImageFile ile ilgili bir hata varsa, bunu temizliyoruz.
            // Çünkü bu alanları isteğe bağlı yaptık, ama ModelState eski bir doğrulama hatasını tutuyor olabilir.
            if (ModelState.ContainsKey("ImageUrl"))
            {
                ModelState["ImageUrl"].Errors.Clear();
                ModelState["ImageUrl"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }
            if (ModelState.ContainsKey("ImageFile"))
            {
                ModelState["ImageFile"].Errors.Clear();
                ModelState["ImageFile"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            // Formdan gelen bilgileri kontrol ediyoruz.
            // Eğer bir hata varsa (örneğin, zorunlu alanlar boşsa), işlemi durduruyoruz.
            if (!ModelState.IsValid)
            {
                // Hataları bulup listeliyoruz ki neyin yanlış olduğunu anlayabilelim.
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("ModelState is invalid. Errors: " + string.Join(", ", errors));
                TempData["ErrorMessage"] = "Formda hatalar var: " + string.Join(", ", errors);

                // Kategorileri tekrar yüklüyoruz ki formda kategori seçenekleri kaybolmasın.
                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            // Veritabanı işlemlerinde bir hata olursa geri alabilmek için bir işlem (transaction) başlatıyoruz.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Yeni bir ürün oluşturuyoruz ve kullanıcının formda girdiği bilgileri dolduruyoruz.
                var product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    StockQuantity = model.StockQuantity,
                    DiscountRate = model.DiscountRate,
                    CampaignMessage = model.CampaignMessage,
                    Colors = model.Colors,
                    Categories = new List<Category>()
                };

                // Eğer kullanıcı bir resim yüklediyse, resmi kaydediyoruz.
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Resmi kaydedeceğimiz klasörü belirliyoruz.
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    // Eğer klasör yoksa, oluşturuyoruz.
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Resme benzersiz bir isim veriyoruz ki çakışma olmasın.
                    string uniqueFileName = $"{DateTime.Now.Ticks}_{model.ImageFile.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Resmi dosya sistemine kaydediyoruz.
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    // Resmin yolunu ürünün ImageUrl alanına kaydediyoruz.
                    product.ImageUrl = $"/images/products/{uniqueFileName}";
                    Console.WriteLine($"Image uploaded: {product.ImageUrl}");
                }
                else
                {
                    // Eğer resim yüklenmediyse, ImageUrl null kalır.
                    Console.WriteLine("No image uploaded, ImageUrl will be null.");
                }

                // Yeni ürünü veritabanına ekliyoruz.
                _context.Products.Add(product);
                // Değişiklikleri veritabanına kaydediyoruz.
                await _context.SaveChangesAsync();
                Console.WriteLine($"Product saved with ID: {product.Id}");

                // Eğer kullanıcı kategori seçtiyse, kategorileri ekliyoruz.
                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    Console.WriteLine($"Selected Category IDs: {string.Join(", ", model.CategoryIds)}");
                    // Seçilen kategorileri veritabanından buluyoruz.
                    var selectedCategories = await _context.Categories
                        .Where(c => model.CategoryIds.Contains(c.Id))
                        .ToListAsync();
                    Console.WriteLine($"Found Categories: {selectedCategories.Count}");
                    if (selectedCategories.Any())
                    {
                        Console.WriteLine($"Categories to be added: {string.Join(", ", selectedCategories.Select(c => c.Name))}");
                        // Her bir kategori için ürün-kategori ilişkisi oluşturuyoruz.
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

                // Eklenen ürünü kontrol etmek için tekrar çekiyoruz.
                var addedProduct = await _context.Products
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
                Console.WriteLine($"Added Product ID: {addedProduct.Id}, Categories: {string.Join(", ", addedProduct.Categories.Select(c => c.Name))}");

                // İşlemi tamamlıyoruz (commit).
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, yapılan değişiklikleri geri alıyoruz (rollback).
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = $"Ürün eklenirken bir hata oluştu: {ex.Message}";
                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            // Her şey başarılıysa, ürün listesi sayfasına yönlendiriyoruz.
            return RedirectToAction("Index");
        }

        // GET: /Admin/EditProduct/5
        // Bu metod, bir ürünü düzenlemek için formu açar. Ürünün bilgilerini getirir ve ekranda gösterir.
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            // Veritabanından, verilen ID’ye sahip ürünü buluyoruz.
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Eğer ürün bulunamazsa, 404 (Bulunamadı) hatası döndürüyoruz.
            if (product == null)
            {
                return NotFound();
            }

            // Ürünün bilgilerini ProductDTO’ya çeviriyoruz ki formda kullanabilelim.
            var model = new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                DiscountRate = product.DiscountRate,
                CampaignMessage = product.CampaignMessage,
                Colors = product.Colors,
                CategoryIds = product.Categories.Select(c => c.Id).ToList(),
                AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync()
            };

            // Düzenleme formunu (EditProduct.cshtml) açıyoruz ve modelimizi gönderiyoruz.
            return View(model);
        }

        // POST: /Admin/EditProduct
        // Bu metod, kullanıcının düzenleme formunu doldurup gönderdiğinde çalışır. Ürünü günceller.
        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductDTO model)
        {
            // Eğer ModelState’te ImageUrl veya ImageFile ile ilgili bir hata varsa, bunu temizliyoruz.
            if (ModelState.ContainsKey("ImageUrl"))
            {
                ModelState["ImageUrl"].Errors.Clear();
                ModelState["ImageUrl"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }
            if (ModelState.ContainsKey("ImageFile"))
            {
                ModelState["ImageFile"].Errors.Clear();
                ModelState["ImageFile"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            // Formdan gelen bilgileri kontrol ediyoruz.
            if (!ModelState.IsValid)
            {
                // Hataları bulup listeliyoruz ki neyin yanlış olduğunu anlayabilelim.
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("ModelState is invalid. Errors: " + string.Join(", ", errors));
                TempData["ErrorMessage"] = "Formda hatalar var: " + string.Join(", ", errors);

                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            // Güncellenecek ürünü veritabanından buluyoruz.
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            // Eğer ürün bulunamazsa, 404 hatası döndürüyoruz.
            if (product == null)
            {
                return NotFound();
            }

            // Veritabanı işlemlerinde bir hata olursa geri alabilmek için bir işlem (transaction) başlatıyoruz.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Ürünün bilgilerini, kullanıcının formda gönderdiği yeni bilgilerle güncelliyoruz.
                product.Name = model.Name;
                product.Price = model.Price;
                product.Description = model.Description;
                product.StockQuantity = model.StockQuantity;
                product.DiscountRate = model.DiscountRate;
                product.CampaignMessage = model.CampaignMessage;
                product.Colors = model.Colors;

                // Eğer kullanıcı yeni bir resim yüklediyse, resmi güncelliyoruz.
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Önce eski resmi siliyoruz (eğer varsa).
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Yeni resmi kaydetmek için bir klasör oluşturuyoruz (eğer yoksa).
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Yeni resme benzersiz bir isim veriyoruz ki çakışma olmasın.
                    string uniqueFileName = $"{DateTime.Now.Ticks}_{model.ImageFile.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Yeni resmi dosya sistemine kaydediyoruz.
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    // Yeni resmin yolunu ürünün ImageUrl alanına kaydediyoruz.
                    product.ImageUrl = $"/images/products/{uniqueFileName}";
                    Console.WriteLine($"Image updated: {product.ImageUrl}");
                }

                // Ürünün eski kategorilerini silip yeni seçilen kategorileri ekliyoruz.
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

                // Değişiklikleri veritabanına kaydediyoruz.
                await _context.SaveChangesAsync();
                // İşlemi tamamlıyoruz (commit).
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, yapılan değişiklikleri geri alıyoruz (rollback).
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = $"Ürün güncellenirken bir hata oluştu: {ex.Message}";
                model.AvailableCategories = await _context.Categories
                    .Select(c => new CategoryDTO { Id = c.Id, Name = c.Name })
                    .ToListAsync();
                return View(model);
            }

            // Her şey başarılıysa, ürün listesi sayfasına yönlendiriyoruz.
            return RedirectToAction("Index");
        }

        // POST: /Admin/DeleteProduct/5
        // Bu metod, bir ürünü siler.
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Veritabanından, verilen ID’ye sahip ürünü buluyoruz.
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Eğer ürün bulunamazsa, 404 hatası döndürüyoruz.
            if (product == null)
            {
                return NotFound();
            }

            // Ürüne bağlı kategorileri siliyoruz.
            var productCategories = _context.Set<ProductCategory>()
                .Where(pc => pc.ProductId == id);
            _context.Set<ProductCategory>().RemoveRange(productCategories);

            // Eğer ürünün resmi varsa, resmi dosya sisteminden siliyoruz.
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Ürünü veritabanından siliyoruz.
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Ürün listesi sayfasına yönlendiriyoruz.
            return RedirectToAction("Index");
        }
    }
}