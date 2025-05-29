using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StoreApp.Data.Concrete
{
    // Veritabanı bağlantısını ve tabloları tanımlayan sınıf
    public class StoreDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {
        }

        // Veritabanı tablolarını temsil eden DbSet’ler
        public DbSet<Product> Products { get; set; } // Ürünler tablosu
        public DbSet<Category> Categories { get; set; } // Kategoriler tablosu
        public DbSet<Order> Orders { get; set; } // Siparişler tablosu
        public DbSet<OrderItem> OrderItems { get; set; } // Sipariş öğeleri tablosu
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product: Price hassasiyeti
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Order: TotalAmount hassasiyeti
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // OrderItem: Price ve TotalPrice hassasiyeti
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Order-OrderItem ilişkisi
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Sipariş silinirse öğeler de silinsin

            // Order-AppUser ilişkisi
            modelBuilder.Entity<Order>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse siparişler silinmesin

            // Product-Category çoktan-çoğa ilişki
            modelBuilder.Entity<Product>()
                .HasMany(e => e.Categories)
                .WithMany(e => e.Products)
                .UsingEntity<ProductCategory>();

            // Category: Url benzersiz index
            modelBuilder.Entity<Category>()
                .HasIndex(u => u.Url)
                .IsUnique();

            // Seed veriler: Ürünler
            modelBuilder.Entity<Product>().HasData(new List<Product>()
            {
                new Product { Id = 1, Name = "İphone 1", Price = 1000, Description = "İphone 1", ImageUrl = "/images/products/iphone1.jpg" },
                new Product { Id = 2, Name = "İphone 2", Price = 2000, Description = "İphone 2", ImageUrl = "/images/products/iphone2.jpg" },
                new Product { Id = 3, Name = "MacOs", Price = 50, Description = "MacOs ", ImageUrl = "/images/products/macos.jpg" },
                new Product { Id = 4, Name = "Çamaşır Makinesi", Price = 4000, Description = "Çamaşır Makinesi", ImageUrl = "/images/products/camasir-makinesi.jpg" },
                new Product { Id = 5, Name = "Honor PC", Price = 5000, Description = "Honor PC", ImageUrl = "/images/products/honor-pc.jpg" },
                new Product { Id = 6, Name = "İphone 6", Price = 6000, Description = "İphone 6", ImageUrl = "/images/products/iphone6.jpg" },
            });

            // Seed veriler: Kategoriler
            modelBuilder.Entity<Category>().HasData(new List<Category>()
            {
                new Category { Id = 1, Name = "Telefon", Url = "telefon" },
                new Category { Id = 2, Name = "Elektronik", Url = "elektronik" },
                new Category { Id = 3, Name = "Beyaz Eşya", Url = "beyaz-esya" },
            });

            // Seed veriler: Ürün-Kategori ilişkileri
            modelBuilder.Entity<ProductCategory>().HasData(new List<ProductCategory>()
            {
                new ProductCategory { ProductId = 1, CategoryId = 1 },
                new ProductCategory { ProductId = 2, CategoryId = 1 },
                new ProductCategory { ProductId = 3, CategoryId = 2 },
                new ProductCategory { ProductId = 4, CategoryId = 3 },
                new ProductCategory { ProductId = 5, CategoryId = 2 },
                new ProductCategory { ProductId = 6, CategoryId = 1 },
            });
        }
    }
}