using System;
using System.Collections.Generic;

namespace StoreApp.Data.Concrete
{
    // Sipariş bilgilerini temsil eden sınıf
    public class Order
    {
        public int Id { get; set; } // Siparişin benzersiz kimliği
        public int? UserId { get; set; } // Siparişi oluşturan kullanıcının kimliği (AppUser.Id, int türünde)
        public DateTime OrderDate { get; set; } // Sipariş tarihi
        public string? Name { get; set; } // Alıcı adı
        public string? City { get; set; } // Şehir
        public string? Phone { get; set; } // Telefon numarası
        public string? Email { get; set; } // E-posta adresi
        public string? AddressLine { get; set; } // Teslimat adresi
        public decimal TotalAmount { get; set; } // Toplam tutar
        public List<OrderItem> OrderItems { get; set; } = new(); // Sipariş öğeleri
        public string? TrackingNumber { get; set; } // Yeni alan: Kargo takip numarası

        public string OrderStatus { get; set; } // beklemede, tamamlandı, iptal
    }

    // Sipariş öğesini temsil eden sınıf
    public class OrderItem
    {
        public int Id { get; set; } // Öğe kimliği
        public int OrderId { get; set; } // İlgili siparişin kimliği (foreign key)
        public Order? Order { get; set; } // İlgili sipariş nesnesi
        public int ProductId { get; set; } // Ürün kimliği
        public Product? Product { get; set; } // İlgili ürün nesnesi
        public string? ProductName { get; set; } // Ürün adı
        public decimal Price { get; set; } // Birim fiyat
        public int Quantity { get; set; } // Miktar
        public decimal TotalPrice { get; set; } // Toplam fiyat (Price * Quantity)
    }
}