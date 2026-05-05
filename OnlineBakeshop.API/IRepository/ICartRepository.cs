using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface ICartRepository
    {
        Task<ServiceResponse<object>> GetCartByUser(int userId);
        Task<ServiceResponse<object>> UpsertCartItem(CartModel cart);
        Task<ServiceResponse<object>> UpdateCartItemQty(int cartId, int userId, int quantity);
        Task<ServiceResponse<object>> RemoveCartItem(int cartId, int userId);
        Task<ServiceResponse<object>> ClearCart(int userId);
    }
}