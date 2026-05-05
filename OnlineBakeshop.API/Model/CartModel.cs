namespace OnlineBakeshop.API.Model
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? DeliveryDate { get; set; }
        public string? DeliveryTime { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? SpecialNotes { get; set; }
        public string? PaymentMethod { get; set; }
        public string? FulfillmentType { get; set; }
        public string? MeetupPlace { get; set; }

        // Joined product fields (returned by GETBYUSER)
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}