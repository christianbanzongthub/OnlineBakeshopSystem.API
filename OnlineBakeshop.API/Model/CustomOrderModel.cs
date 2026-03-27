namespace OnlineBakeshop.API.Model
{
    public class CustomOrderModel
    {
        public int CustomOrderId { get; set; }
        public int UserId { get; set; }
        public string OrderType { get; set; }
        public string Flavor { get; set; }
        public string Size { get; set; }
        public string ColorTheme { get; set; }
        public string MessageOnCake { get; set; }
        public int? NumberOfLayers { get; set; }
        public string ReferenceImage { get; set; }
        public string SpecialNotes { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
        public DateTime DateOrdered { get; set; }

        public decimal? QuotedPrice { get; set; }

        // FROM JOIN
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
    }
}