using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IAdminRepository
    {
        Task<ServiceResponse<object>> AdminLogin(AdminLoginModel admin);
        Task<ServiceResponse<object>> GetAllAdmins();
        Task<ServiceResponse<object>> GetAdminById(int adminId);
        Task<ServiceResponse<object>> CreateAdmin(AdminModel admin);
        Task<ServiceResponse<object>> UpdateAdmin(AdminModel admin);
        Task<ServiceResponse<object>> DeleteAdmin(int adminId);
    }
}