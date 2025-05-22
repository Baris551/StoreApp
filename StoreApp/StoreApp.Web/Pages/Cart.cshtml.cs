using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreApp.Data.Abstract;
using StoreApp.Web.Helpers;
using StoreApp.Web.Models;

namespace StoreApp.Web.Pages;

public class CartModel : PageModel
{
    private readonly IStoreRepository _storeRepository;

    public CartModel(IStoreRepository storeRepository, Cart cartService )
    {
        _storeRepository = storeRepository;
        Cart = cartService;
    }

    public Cart Cart { get; set; } = new Cart();

    public void OnGet()
    {
        // Session'dan cart'ı alıyoruz. Eğer yoksa yeni bir cart oluşturuyoruz.
        // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var product = await _storeRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        // HttpContext.Session.SetJson("cart", Cart);
        Cart?.AddItem(product, 1);

        return RedirectToPage("/Cart");
    }

    public IActionResult OnPostRemove(int productId)
    {
        // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        var product = Cart.CartItems.FirstOrDefault(p => p.Product?.Id == productId)?.Product;
        if (product != null)
        {
            Cart.RemoveItem(product);
            HttpContext.Session.SetJson("cart", Cart);
        }

        return RedirectToPage();
    }
}