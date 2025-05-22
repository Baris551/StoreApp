namespace StoreApp.Web.Models
{
    public class CheckoutCompletedViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string AddressLine { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}