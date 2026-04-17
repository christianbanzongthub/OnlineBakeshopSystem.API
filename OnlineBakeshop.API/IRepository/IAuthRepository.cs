using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<object>> RefreshToken(string refreshToken);
        Task<ServiceResponse<object>> RevokeToken(string refreshToken);
    }
}