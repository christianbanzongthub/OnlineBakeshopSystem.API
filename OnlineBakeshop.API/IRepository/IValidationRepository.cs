using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IValidationRepository
    {
        Task<ServiceResponse<ValidationModel>> ValidateOrder(int userId, int productId, int quantity);
        Task<ServiceResponse<ValidationModel>> ValidateProduct(string productName, decimal price);
        Task<ServiceResponse<ValidationModel>> ValidateUser(string fullName, string email, string password, string contactNo);
    }
}