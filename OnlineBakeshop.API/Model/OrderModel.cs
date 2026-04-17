namespace OnlineBakeshop.API.Model
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }

        // Delivery fields (added for mobile ordering)
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string SpecialNotes { get; set; }

        // From JOIN (returned by GETALL / GETBYID / GETBYUSER)
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
    }
}