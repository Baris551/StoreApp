using System;
using Microsoft.AspNetCore.Mvc;
using StoreApp.Web.Models;

namespace StoreApp.Web.Components;

public class CartSummeryViewComponent : ViewComponent
{
    private Cart _Cart;

    public CartSummeryViewComponent(Cart cartService)
    {
        _Cart = cartService;
    }

    public IViewComponentResult Invoke()
    {
        return View(_Cart);
    }
}
