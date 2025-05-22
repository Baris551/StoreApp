namespace StoreApp.Web.Models;

public class CategoryViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; } // Category Telefon categorysi ise Telefon => telefon ,Beyaz Esya => beyaz-esya gibi bir url oluşturmak için kullanılır.
    
}