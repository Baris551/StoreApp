using System;
using StoreApp.Data.Concrete;

namespace StoreApp.Data.Abstract;
// Veritabanındaki ürünlerle (Product) ilgili işlemleri tanımlayan bir arayüzdür.
// Uygulamanın veri erişim katmanında kullanılır.

public interface IStoreRepository
{
    IQueryable<Product> Products { get; }
    IQueryable<Category> Categories { get; }
    void CreateProduct(Product entity);
    Task CreateProductAsync(Product product, List<int> categoryIds = null);
    Task<int> GetProductCountAsync(string category);
    Task<IQueryable<Product>> GetProductsByCategoryAsync(string category, int page, int pageSize, string sort = null);
    Task<Product> GetProductByIdAsync(int id);
}

//IStoreRepository arayüzü, StoreApp adlı uygulamada ürünlerle ilgili veritabanı işlemlerinin (ürünleri listeleme, yeni ürün ekleme gibi) standart bir şekilde tanımlanmasını sağlar. Bu arayüz, veri erişim katmanında (Data Layer) yer alır ve uygulamanın diğer katmanlarıyla (örneğin iş mantığı veya kullanıcı arayüzü) veritabanı arasındaki bağlantıyı soyutlamak için kullanılır. Böylece farklı veri kaynaklarına geçiş yapmak veya test amacıyla sahte (mock) veri kullanmak kolaylaşır.