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
    }
}
