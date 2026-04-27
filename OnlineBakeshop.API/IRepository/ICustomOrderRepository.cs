using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface ICustomOrderRepository
    {
        Task<ServiceResponse<List<CustomOrderModel>>> GetAllCustomOrders();
        Task<ServiceResponse<CustomOrderModel>> GetCustomOrderById(int customOrderId);
        Task<ServiceResponse<object>> CreateCustomOrder(CustomOrderModel order);
        Task<ServiceResponse<object>> UpdateStatus(int customOrderId, string orderStatus);
        Task<ServiceResponse<object>> MarkAsPaid(int customOrderId);
        Task<ServiceResponse<object>> RejectCustomOrder(int customOrderId);
        Task<ServiceResponse<object>> DeleteCustomOrder(int customOrderId);
        Task<ServiceResponse<object>> SetPrice(int customOrderId, decimal quotedPrice);
    }
}