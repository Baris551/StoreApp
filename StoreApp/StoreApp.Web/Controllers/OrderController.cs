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
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly Cart _cart;
        private readonly IMapper _mapper;

        public OrderController(IOrderRepository orderRepository, Cart cartService, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cart = cartService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            if (!_cart.CartItems.Any())
            {
                TempData["Error"] = "Sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Index", "Cart");
            }

            var cartModel = new OrderViewModel { Cart = _cart };
            return View(cartModel);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            if (!_cart.CartItems.Any())
            {
                ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır.");
                model.Cart = _cart;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.Cart = _cart;
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
            {
                ModelState.AddModelError("Email", "Geçerli bir e-posta adresi girin.");
                model.Cart = _cart;
                return View(model);
            }

            var order = _mapper.Map<Order>(model);
            order.TotalAmount = _cart.CalculateTotal();
            order.OrderStatus = "Pending";
            order.OrderDate = DateTime.Now;

            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                order.UserId = userId;

            if (!order.OrderItems.Any() && _cart.CartItems.Any())
            {
                order.OrderItems = _cart.CartItems.Select(ci => new StoreApp.Data.Concrete.OrderItem
                {
                    ProductId = ci.Product?.Id ?? 0,
                    ProductName = ci.Product?.Name ?? "Bilinmeyen Ürün",
                    Price = ci.Product?.Price ?? 0m,
                    Quantity = ci.Quantity,
                    TotalPrice = (ci.Product?.Price ?? 0m) * ci.Quantity
                }).ToList();
            }

            model.Cart = _cart;
            var payment = await ProcessPayment(order, model);

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

            order.OrderStatus = "Completed";
            await _orderRepository.CreateOrderAsync(order);
            _cart.Clear();

            return RedirectToAction("CheckoutCompleted", new { orderId = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> CheckoutCompleted(int? orderId)
        {
            Console.WriteLine($"CheckoutCompleted called with orderId: {orderId}");
            if (!orderId.HasValue)
            {
                TempData["Error"] = "Geçersiz sipariş numarası.";
                return RedirectToAction("Index", "Orders");
            }

            var order = await _orderRepository.GetOrderByIdAsync(orderId.Value);
            Console.WriteLine($"Order found: {order != null}, OrderId: {orderId}");
            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction("Index", "Orders");
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"UserId: {userId}, Order UserId: {order.UserId}, IsAdmin: {User.IsInRole("Admin")}");
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Bu siparişe erişim yetkiniz yok.";
                return RedirectToAction("Index", "Orders");
            }

            return View(order);
        }

        private async Task<Payment> ProcessPayment(Order order, OrderViewModel model)
        {
            Options options = new Options
            {
                ApiKey = "sandbox-57mMWtI5dKvYxZU7A6tbNhWViGKOYXRa",
                SecretKey = "sandbox-AdAM26MJzDtpUB3GNbQ8pZQ3U04fqlg4",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };

            CreatePaymentRequest request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = new Random().Next(111111111, 999999999).ToString() + order.Id.ToString(),
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                BasketId = $"B{order.Id}",
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString()
            };

            if (string.IsNullOrEmpty(model.CartName) || string.IsNullOrEmpty(model.CartNumber) ||
                string.IsNullOrEmpty(model.ExpirationMonth) || string.IsNullOrEmpty(model.ExpirationYear) ||
                string.IsNullOrEmpty(model.Cvc))
            {
                return new Payment { Status = "failure", ErrorMessage = "Ödeme kartı bilgileri eksik." };
            }

            request.PaymentCard = new PaymentCard
            {
                CardHolderName = model.CartName,
                CardNumber = model.CartNumber,
                ExpireMonth = model.ExpirationMonth,
                ExpireYear = model.ExpirationYear,
                Cvc = model.Cvc,
                RegisterCard = 0
            };

            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Phone) ||
                string.IsNullOrEmpty(model.City) || string.IsNullOrEmpty(model.AddressLine))
            {
                return new Payment { Status = "failure", ErrorMessage = "Teslimat bilgileri eksik." };
            }

            request.Buyer = new Buyer
            {
                Id = User.Identity.Name,
                Name = model.Name,
                Surname = "Soyadı",
                GsmNumber = model.Phone,
                Email = model.Email,
                IdentityNumber = "74300864791",
                LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationAddress = model.AddressLine,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                City = model.City,
                Country = "Turkey",
                ZipCode = "34732"
            };

            request.ShippingAddress = new Address
            {
                ContactName = model.Name,
                City = model.City,
                Country = "Turkey",
                Description = model.AddressLine,
                ZipCode = "34742"
            };

            request.BillingAddress = new Address
            {
                ContactName = model.Name,
                City = model.City,
                Country = "Turkey",
                Description = model.AddressLine,
                ZipCode = "34742"
            };

            if (!order.OrderItems.Any())
                return new Payment { Status = "failure", ErrorMessage = "Sipariş öğeleri boş." };

            if (order.TotalAmount <= 0)
                return new Payment { Status = "failure", ErrorMessage = "Toplam tutar sıfır veya negatif olamaz." };

            List<BasketItem> basketItems = new List<BasketItem>();
            foreach (var item in order.OrderItems)
            {
                if (item.Price <= 0 || item.Quantity <= 0)
                    return new Payment { Status = "failure", ErrorMessage = $"Geçersiz öğe: {item.ProductName}" };

                if (string.IsNullOrEmpty(item.ProductName))
                    item.ProductName = "Bilinmeyen Ürün";

                basketItems.Add(new BasketItem
                {
                    Id = item.ProductId.ToString(),
                    Name = item.ProductName,
                    Category1 = "Ürün",
                    Category2 = "Genel",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (item.Price * item.Quantity).ToString("F2", CultureInfo.InvariantCulture)
                });
            }

            request.BasketItems = basketItems;
            request.Price = order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
            request.PaidPrice = order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture);

            return await Payment.Create(request, options);
        }
    }
}