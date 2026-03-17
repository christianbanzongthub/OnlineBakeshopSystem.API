using OnlineBakeshop.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderRepository
{
    Task<int> CreateOrder(OrderModel order);
    Task<List<OrderModel>> GetAllOrders();
    Task<OrderModel> GetOrderById(int orderId);
    Task UpdateOrder(OrderModel order);
    Task DeleteOrder(int orderId);
}