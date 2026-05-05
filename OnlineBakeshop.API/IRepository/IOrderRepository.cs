using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;
namespace OnlineBakeshop.API.IRepository
{
    public interface IOrderRepository
    {
        Task<ServiceResponse<object>> CreateOrder(OrderModel order);
        Task<ServiceResponse<List<OrderModel>>> GetAllOrders();
        Task<ServiceResponse<List<OrderModel>>> GetOrdersByUserId(int userId);
        Task<ServiceResponse<OrderModel>> GetOrderById(int orderId);
        Task<ServiceResponse<object>> UpdateOrder(OrderModel order);
        Task<ServiceResponse<object>> UpdateReceipt(int orderId, string receiptImagePath);
        Task<ServiceResponse<object>> ConfirmPayment(int orderId);
        Task<ServiceResponse<object>> RejectOrder(int orderId);
        Task<ServiceResponse<object>> DeleteOrder(int orderId);
        Task<ServiceResponse<object>> CancelOrder(int orderId); // NEW
    }
}