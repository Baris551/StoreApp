using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StoreApp.Web.Models;

public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine { get; set; }

    [BindNever] // Özet
    public Cart? Cart { get; set; }


    public string CartName { get; set; }
    
    public string CartNumber { get; set; }

    public string ExpirationMonth { get; set; }

    public string ExpirationYear { get; set; }
    
    public string Cvc { get; set; }
    



    
}

