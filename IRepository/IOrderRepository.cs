using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IOrderRepository
    {
        Task<ServiceResponse<object>> CreateOrder(OrderModel order);
        Task<ServiceResponse<object>> GetAllOrders();
        Task<ServiceResponse<object>> GetOrderById(int orderId);
        Task<ServiceResponse<object>> UpdateOrder(OrderModel order);
        Task<ServiceResponse<object>> DeleteOrder(int orderId);
    }
}