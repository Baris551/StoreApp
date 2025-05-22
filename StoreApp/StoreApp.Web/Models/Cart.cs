using StoreApp.Data.Concrete;

namespace StoreApp.Web.Models;

public class Cart
{
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    //virtyal keyword'ü ile override edilebilir hale getirildi. virtual = ezilebilir
    public virtual void AddItem(Product product, int quantity)
    {
        if (product == null) return;

        var item = CartItems.FirstOrDefault(p => p.Product?.Id == product.Id);
        if (item == null)
        {
            CartItems.Add(new CartItem { Product = product, Quantity = quantity });
        }
        else
        {
            item.Quantity += quantity;
        }
    }

    public virtual void RemoveItem(Product product)
    {
        if (product == null) return;

        var item = CartItems.FirstOrDefault(p => p.Product?.Id == product.Id);
        if (item != null)
        {
            item.Quantity -= 1; // Miktarı bir azalt
            if (item.Quantity <= 0)
            {
                CartItems.Remove(item); // Miktar 0 veya altına düşerse ürünü tamamen kaldır
            }
        }
    }

    public decimal CalculateTotal()
    {
        return CartItems.Sum(p => (p.Product?.Price ?? 0) * p.Quantity);
    }

    public virtual void Clear()
    {
        CartItems.Clear();
    }
}

public class CartItem
{
    public int CartItemId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
}