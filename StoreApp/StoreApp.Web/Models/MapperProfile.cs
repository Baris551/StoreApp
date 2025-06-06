using AutoMapper;
using Microsoft.Extensions.Logging;
using StoreApp.Data.Concrete;
using StoreApp.Web.Models;

namespace StoreApp.Web.Models
{
    public class MapperProfile : Profile
    {
        private readonly ILogger<MapperProfile>? _logger; // Nullable hale getirildi

        // Parametresiz yapıcı
        public MapperProfile()
        {
            ConfigureMappings();
        }

        // ILogger ile yapıcı (opsiyonel)
        public MapperProfile(ILogger<MapperProfile> logger) : this()
        {
            _logger = logger;
        }

        private void ConfigureMappings()
        {
            // Product -> ProductViewModel
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.DiscountRate, opt => opt.MapFrom(src => src.DiscountRate ?? 0))
                .ForMember(dest => dest.CampaignMessage, opt => opt.MapFrom(src => src.CampaignMessage ?? string.Empty))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl ?? "/images/default-product.jpg"))
                .ForMember(dest => dest.Colors, opt => opt.MapFrom(src => src.Colors != null ? src.Colors.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()));

            // OrderViewModel -> Order
            CreateMap<OrderViewModel, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.Cart != null && src.Cart.CartItems != null ? src.Cart.CartItems : new List<CartItem>()))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.AddressLine, opt => opt.MapFrom(src => src.AddressLine));

            // CartItem -> OrderItem
            CreateMap<CartItem, OrderItem>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product != null ? src.Product.Id : 0))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0m))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Bilinmeyen Ürün"))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price * src.Quantity : 0m))
                .AfterMap((src, dest) =>
                {
                    _logger?.LogInformation($"CartItem’dan OrderItem’a dönüşüm - ProductName: {dest.ProductName}, Quantity: {dest.Quantity}, Price: {dest.Price}");
                });

            // Order -> CheckoutCompletedViewModel
            CreateMap<Order, CheckoutCompletedViewModel>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.AddressLine, opt => opt.MapFrom(src => src.AddressLine))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems ?? new List<OrderItem>()));

            // OrderItem -> OrderItemViewModel
            CreateMap<OrderItem, OrderItemViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName ?? "Bilinmeyen Ürün"));
        }
    }
}