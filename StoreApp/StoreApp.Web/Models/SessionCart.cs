using System;
using System.Text.Json.Serialization;
using StoreApp.Data.Concrete;
using StoreApp.Web.Helpers;

namespace StoreApp.Web.Models;

public class SessionCart:Cart
{

    public static Cart GetCart(IServiceProvider services)
    {
        ISession session = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Session;
        SessionCart cart = session?.GetJson<SessionCart>("Cart") ?? new SessionCart();
        cart.Session = session;
        return cart;
    }
    
    // session eklerken sitelize etmenize gerek yok. 
    [JsonIgnore]
    public ISession Session { get; set; }

    // override .
    public override void AddItem(Product product, int quantity)
    {
        base.AddItem(product, quantity);
        //session ekle
        Session?.SetJson("Cart", this);
    }
    public override void RemoveItem(Product product)
    {
        base.RemoveItem(product);
        Session?.SetJson("Cart", this);

    }
    public override void Clear()
    {
        base.Clear();
        Session?.Remove("Cart");

    }
}
