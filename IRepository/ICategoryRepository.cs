// Location: OnlineBakeshop.API/IRepository/ICategoryRepository.cs

using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface ICategoryRepository
    {
        Task<ServiceResponse<object>> GetCategories();
        Task<ServiceResponse<object>> GetCategoryById(int categoryId);
        Task<ServiceResponse<object>> CreateCategory(CategoryModel category);
        Task<ServiceResponse<object>> UpdateCategory(CategoryModel category);
        Task<ServiceResponse<object>> DeleteCategory(int categoryId);
    }
}