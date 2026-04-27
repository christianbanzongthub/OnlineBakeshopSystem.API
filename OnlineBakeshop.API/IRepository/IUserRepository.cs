using OnlineBakeshop.API.Model;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IUserRepository
    {
        Task<ServiceResponse<object>> GetAllUsers();
        Task<ServiceResponse<object>> GetUserById(int userId);
        Task<ServiceResponse<object>> UpdateUser(UserModel user);
        Task<ServiceResponse<object>> UpdateProfilePicture(int userId, string profilePictureUrl);
        Task<ServiceResponse<object>> DeleteUser(int userId);
    }
}