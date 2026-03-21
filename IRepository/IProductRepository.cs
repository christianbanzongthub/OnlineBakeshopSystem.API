using OnlineBakeshop.API.Model.Response;
using System.Threading.Tasks;

namespace OnlineBakeshop.API.IRepository
{
    public interface IProductRepository
    {
        Task<ServiceResponse<object>> GetProducts();

        Task<ServiceResponse<object>> CreateProduct( string productName, string description, decimal price, string imageUrl, bool isAvailable);
    }
}