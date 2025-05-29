using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Data.Abstract;
using StoreApp.Data.Concrete;
using StoreApp.Web.Models;
using System.Security.Claims;
using System.Globalization;

namespace StoreApp.Web.Controllers
{
    // [Authorize] atributu: Bu controller'a sadece giriş yapmış kullanıcılar erişebilir.
    [Authorize]
    public class OrderController : Controller
    {
        // Sipariş işlemlerini yapan repository
        private readonly IOrderRepository _orderRepository;
        // Kullanıcının sepetini yöneten servis
        private readonly Cart _cart;
        // Nesneler arasında veri dönüşümünü sağlayan AutoMapper
        private readonly IMapper _mapper;

        // Constructor: Bağımlılıkları repository, cart, mapper
        public OrderController(IOrderRepository orderRepository, Cart cartService, IMapper mapper)
        {
            _orderRepository = orderRepository; // Sipariş işlemlerini başlatır
            _cart = cartService;               // Sepet işlemlerini başlatır
            _mapper = mapper;                  // AutoMapper'ı başlatır
        }

        // GET: Ödeme sayfasını gösterir
        [HttpGet]
        public IActionResult Checkout()
        {
            // Sepette ürün yoksa hata mesajı göster ve sepet sayfasına yönlendir
            if (!_cart.CartItems.Any())
            {
                TempData["Error"] = "Sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Index", "Cart");
            }

            // Ödeme sayfasında sepet bilgilerini göstermek için bir model oluştur
            var cartModel = new OrderViewModel { Cart = _cart };
            return View(cartModel); // Ödeme sayfasını döndür
        }

        // POST: Ödeme işlemini gerçekleştirir
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            // Sepette ürün yoksa hata mesajı ekle ve ödeme sayfasını tekrar göster
            if (!_cart.CartItems.Any())
            {
                ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır.");
                model.Cart = _cart;
                return View(model);
            }

            // Model geçerli değilse (örneğin, zorunlu alanlar eksikse) ödeme sayfasını tekrar göster
            if (!ModelState.IsValid)
            {
                model.Cart = _cart;
                return View(model);
            }


            // OrderViewModel'i Order nesnesine dönüştür
            var order = _mapper.Map<Order>(model);
            order.TotalAmount = _cart.CalculateTotal(); // Sepet toplamını hesapla
            order.OrderStatus = "Pending"; // Sipariş durumunu "Bekliyor" olarak ayarla
            order.OrderDate = DateTime.Now; // Sipariş tarihini şu anki zaman olarak ayarla

            // Kullanıcı ID'sini al ve siparişe ekle
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                order.UserId = userId;

            // Sepet öğelerinden sipariş öğelerini oluştur
            if (!order.OrderItems.Any() && _cart.CartItems.Any())
            {
                order.OrderItems = _cart.CartItems.Select(ci => new StoreApp.Data.Concrete.OrderItem
                {
                    ProductId = ci.Product?.Id ?? 0, 
                    ProductName = ci.Product?.Name ?? "Bilinmeyen Ürün", // Ürün adı
                    Price = ci.Product?.Price ?? 0m, 
                    Quantity = ci.Quantity, 
                    TotalPrice = (ci.Product?.Price ?? 0m) * ci.Quantity 
                }).ToList();
            }

            // Modeldeki sepet bilgilerini güncelle
            model.Cart = _cart;
            // Ödeme işlemini başlat
            var payment = await ProcessPayment(order, model);

            // Ödeme başarısızsa hata mesajı göster ve ödeme sayfasını tekrar yükle
            if (payment.Status != "success")
            {
                string errorMessage = $"Ödeme işlemi başarısız oldu: {payment.ErrorMessage}";
                if (!string.IsNullOrEmpty(payment.ErrorCode))
                    errorMessage += $" (Hata Kodu: {payment.ErrorCode})";
                if (!string.IsNullOrEmpty(payment.ErrorGroup))
                    errorMessage += $" (Hata Grubu: {payment.ErrorGroup})";

                ModelState.AddModelError("", errorMessage);
                model.Cart = _cart;
                return View(model);
            }

            // Ödeme başarılıysa sipariş durumunu "Tamamlandı" yap , kaydet
            order.OrderStatus = "Completed";
            await _orderRepository.CreateOrderAsync(order);
            _cart.Clear(); // Sepeti temizle

            // Ödeme tamamlandıya yönlendir
            return RedirectToAction("CheckoutCompleted", new { orderId = order.Id });
        }

        // GET: Ödeme tamamlandı gösterir
        [HttpGet]
        public async Task<IActionResult> CheckoutCompleted(int? orderId)
        {
            // Konsola sipariş ID'sini yaz (hata ayıklama için)
            Console.WriteLine($"CheckoutCompleted called with orderId: {orderId}");
            // Sipariş ID'si yoksa hata göster ve siparişler sayfasına yönlendir
            if (!orderId.HasValue)
            {
                TempData["Error"] = "Geçersiz sipariş numarası.";
                return RedirectToAction("Index", "Orders");
            }

            // Siparişi veritabanından getir
            var order = await _orderRepository.GetOrderByIdAsync(orderId.Value);
            Console.WriteLine($"Order found: {order != null}, OrderId: {orderId}");
            // Sipariş bulunamazsa hata göster ve siparişler sayfasına yönlendir
            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index", "Orders");
            }

            // Kullanıcı ID'sini al
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"UserId: {userId}, Order UserId: {order.UserId}, IsAdmin: {User.IsInRole("Admin")}");
            // Kullanıcı siparişin sahibi değilse ve Admin değilse, erişim hatası göster
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bu siparişe erişim yetkiniz yok.";
                return RedirectToAction("Index", "Orders");
            }

            // Sipariş detaylarını görünümle döndür
            return View(order);
        }

        // Ödeme işlemini gerçekleştiren  metod
        private async Task<Payment> ProcessPayment(Order order, OrderViewModel model)
        {
            // İyzico ödeme servisi için ayarlar
            Options options = new Options
            {
                ApiKey = "", // Test API anahtarı
                SecretKey = "", // Test gizli anahtar
                BaseUrl = "" // Test ortamı URL'si
            };

            // Ödeme isteği oluştur
            CreatePaymentRequest request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(), //  Türkçe
                ConversationId = new Random().Next(111111111, 999999999).ToString() + order.Id.ToString(), // Benzersiz işlem ID'si
                Currency = Currency.TRY.ToString(), //  Türk Lirası
                Installment = 1, // Taksit sayısı
                BasketId = $"B{order.Id}", // Sepet ID'si
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString() 
            };

            // Ödeme kartı bilgileri eksikse hata döndür
            if (string.IsNullOrEmpty(model.CartName) || string.IsNullOrEmpty(model.CartNumber) ||
                string.IsNullOrEmpty(model.ExpirationMonth) || string.IsNullOrEmpty(model.ExpirationYear) ||
                string.IsNullOrEmpty(model.Cvc))
            {
                return new Payment { Status = "failure", ErrorMessage = "Ödeme kartı bilgileri eksik." };
            }

            // Ödeme kartı bilgilerini ayarla
            request.PaymentCard = new PaymentCard
            {
                CardHolderName = model.CartName, // Kart sahibi adı
                CardNumber = model.CartNumber, // Kart numarası
                ExpireMonth = model.ExpirationMonth, // Son kullanma ayı
                ExpireYear = model.ExpirationYear, // Son kullanma yılı
                Cvc = model.Cvc, // CVC kodu
                RegisterCard = 0 // Kartı kaydetme
            };

            // Teslimat bilgileri eksikse hata döndür
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Phone) ||
                string.IsNullOrEmpty(model.City) || string.IsNullOrEmpty(model.AddressLine))
            {
                return new Payment { Status = "failure", ErrorMessage = "Teslimat bilgileri eksik." };
            }

            // Alıcı bilgilerini ayarla
            request.Buyer = new Buyer
            {
                Id = User.Identity.Name, // Kullanıcı adı
                Name = model.Name, // Alıcı adı
                Surname = "Soyadı", // Sabit soyadı (örnek)
                GsmNumber = model.Phone, // Telefon numarası
                Email = model.Email, // E-posta adresi
                IdentityNumber = "74300864791", // Sabit kimlik numarası (örnek)
                LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Son giriş tarihi
                RegistrationDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd HH:mm:ss"), // Kayıt tarihi
                RegistrationAddress = model.AddressLine, // Kayıt adresi
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1", // Kullanıcı IP adresi
                City = model.City, // Şehir
                Country = "Turkey", // Ülke
                ZipCode = "34732" // Posta kodu
            };

            // Kargo adresini ayarla
            request.ShippingAddress = new Address
            {
                ContactName = model.Name, // İlgili kişi adı
                City = model.City, // Şehir
                Country = "Turkey", // Ülke
                Description = model.AddressLine, // Adres detayı
                ZipCode = "34742" // Posta kodu
            };

            // Fatura adresini ayarla
            request.BillingAddress = new Address
            {
                ContactName = model.Name, // İlgili kişi adı
                City = model.City, // Şehir
                Country = "Turkey", // Ülke
                Description = model.AddressLine, // Adres detayı
                ZipCode = "34742" // Posta kodu
            };

            // Sipariş öğeleri boşsa hata döndür
            if (!order.OrderItems.Any())
                return new Payment { Status = "failure", ErrorMessage = "Sipariş öğeleri boş." };

            // Toplam tutar sıfır veya negatifse hata döndür
            if (order.TotalAmount <= 0)
                return new Payment { Status = "failure", ErrorMessage = "Toplam tutar sıfır veya negatif olamaz." };

            // Sepet öğelerini ödeme isteğine ekle
            List<BasketItem> basketItems = new List<BasketItem>();
            foreach (var item in order.OrderItems)
            {
                // Ürün fiyatı veya miktarı geçersizse hata döndür
                if (item.Price <= 0 || item.Quantity <= 0)
                    return new Payment { Status = "failure", ErrorMessage = $"Geçersiz öğe: {item.ProductName}" };

                // Ürün adı boşsa varsayılan bir ad ata
                if (string.IsNullOrEmpty(item.ProductName))
                    item.ProductName = "Bilinmeyen Ürün";

                // Sepet öğesini ekle
                basketItems.Add(new BasketItem
                {
                    Id = item.ProductId.ToString(), // Ürün ID'si
                    Name = item.ProductName, // Ürün adı
                    Category1 = "Ürün", // Kategori 1
                    Category2 = "Genel", // Kategori 2
                    ItemType = BasketItemType.PHYSICAL.ToString(), // Ürün tipi: Fiziksel
                    Price = (item.Price * item.Quantity).ToString("F2", CultureInfo.InvariantCulture) // Toplam fiyat
                });
            }

            // Ödeme isteğine sepet öğelerini ve tutarları ekle
            request.BasketItems = basketItems;
            request.Price = order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture); // Ödeme tutarı
            request.PaidPrice = order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture); // Ödenen tutar

            // İyzico üzerinden ödeme işlemini gerçekleştir
            return await Payment.Create(request, options);
        }
    }
}