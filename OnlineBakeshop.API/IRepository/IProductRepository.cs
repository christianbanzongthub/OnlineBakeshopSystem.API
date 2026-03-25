using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IProductRepository
    {
        Task<ServiceResponse<object>> GetAllProducts();
        Task<ServiceResponse<object>> GetProductById(int productId);
        Task<ServiceResponse<object>> CreateProduct(ProductModel product);
        Task<ServiceResponse<object>> UpdateProduct(ProductModel product);
        Task<ServiceResponse<object>> DeleteProduct(int productId);
    }
}